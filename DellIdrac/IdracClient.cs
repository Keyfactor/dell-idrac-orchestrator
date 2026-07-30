// Copyright 2024 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Keyfactor.Extensions.Orchestrator.IDRAC
{
    internal class IdracClient
    {
        ILogger logger;

        private string racadmPath;
        private string IP;
        private string user;
        private string password;

        internal IdracClient(string racadmPath, string IP, string user, string password)
        {
            logger = LogHandler.GetClassLogger(this.GetType());

            this.racadmPath = racadmPath;
            this.IP = IP;
            this.user = user;
            this.password = password;
        }

        // RACADM takes a "type" parameter with values 1-10 (server cert, trust root, etc) and outputs to a file.
        internal List<CurrentInventoryItem> GetCerts(int i)
        {
            logger.MethodEntry();

            string certFileName = Path.GetTempPath() + Guid.NewGuid().ToString().Replace("-",string.Empty) + ".txt";

            try
            {
                runRacadm($"sslcertdownload -t {i} -f \"{certFileName}\"");

                List<string> fileContents = System.IO.File.ReadAllLines($"{certFileName}").ToList();
                fileContents.RemoveAll(l => l.StartsWith("#"));
                string[] certs = string.Join('\n', fileContents).Split("-----BEGIN CERTIFICATE-----", StringSplitOptions.RemoveEmptyEntries).Select(x => "-----BEGIN CERTIFICATE-----" + x).ToArray();
                return certs.Select(c => new CurrentInventoryItem()
                {
                    Alias = $"{i}.{Array.IndexOf(certs, c)}",
                    Certificates = new List<string>() { c },
                    PrivateKeyEntry = true
                }).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                logger.LogTrace(e.StackTrace);
                return null;
            }
            finally
            {
                try
                {
                    File.Delete(certFileName);
                }
                catch { }
                logger.MethodExit();
            }
        }

        internal JobResult AddCert(ManagementJobConfiguration config)
        {
            logger.MethodEntry();

            string tempPath = Path.GetTempPath();

            if (!config.Overwrite && GetCerts(1).Count > 0)
            {
                throw new Exception($"Error attempting to add certificate. Certificate for store type 1 already exists and Overwrite not selected.  Please make sure to set Overwrite=true and reschedule job.");
            }

            //logger.LogTrace($"***B64 PFX CERT***: {config.JobCertificate.Contents}");
            //logger.LogTrace($"***CERT PSWD***: {config.JobCertificate.PrivateKeyPassword}");
            (string cert, string key) = GetPemFromPFX(config.JobCertificate.Contents, config.JobCertificate.PrivateKeyPassword);
            string salt = new Random().Next().ToString();

            try
            {
                File.WriteAllText($"{tempPath}uploadkey{salt}.txt", key);
                File.WriteAllText($"{tempPath}uploadcert{salt}.txt", cert);

                runRacadm($"sslkeyupload -t 1 -f \"{tempPath}uploadkey{salt}.txt\"");
                runRacadm($"sslcertupload -t 1 -f \"{tempPath}uploadcert{salt}.txt\"");
                runRacadm($"racreset");

                return new JobResult()
                {
                    Result = OrchestratorJobStatusJobResult.Success,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = ""
                };
            }
            finally
            {
                try
                {
                    File.Delete($"{tempPath}uploadkey{salt}.txt");
                    File.Delete($"{tempPath}uploadcert{salt}.txt");
                }
                catch { }
                finally
                {
                    logger.MethodExit();
                }
            }
        }

        private (string, string) GetPemFromPFX(string pfx, string pfxPassword)
        {
            logger.MethodEntry();

            byte[] pfxBytes = Convert.FromBase64String(pfx);
            //logger.LogTrace($"***GetPemFromPFX B64 PFX CERT***: {pfx}");
            //logger.LogTrace($"***GetPemFromPFX CERT PSWD***: {pfx}");
            //logger.LogTrace($"***GetPemFromPFX CERT BYTE LENGTH***: {pfxBytes.Length.ToString()}");
            Pkcs12Store p = new Pkcs12Store(new MemoryStream(pfxBytes), pfxPassword.ToCharArray());

            // Extract private key
            MemoryStream memoryStream = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(memoryStream);
            PemWriter pemWriter = new PemWriter(streamWriter);

            String alias = (p.Aliases.Cast<String>()).SingleOrDefault(a => p.IsKeyEntry(a));
            AsymmetricKeyParameter publicKey = p.GetCertificate(alias).Certificate.GetPublicKey();
            if (p.GetKey(alias) == null) { throw new Exception($"Unable to get the key for alias: {alias}"); }
            AsymmetricKeyParameter privateKey = p.GetKey(alias).Key;
            AsymmetricCipherKeyPair keyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);

            pemWriter.WriteObject(keyPair.Private);
            streamWriter.Flush();
            String privateKeyString = Encoding.ASCII.GetString(memoryStream.GetBuffer()).Trim().Replace("\r", "").Replace("\0", "");
            memoryStream.Close();
            streamWriter.Close();

            // Extract server certificate
            String certStart = "-----BEGIN CERTIFICATE-----\n";
            String certEnd = "\n-----END CERTIFICATE-----";

            Func<String, String> pemify = null;
            pemify = (ss => ss.Length <= 64 ? ss : ss.Substring(0, 64) + "\n" + pemify(ss.Substring(64)));

            string certPem = string.Empty;
            foreach (X509CertificateEntry certEntry in p.GetCertificateChain(alias))
            {
                if (certEntry.Certificate.IssuerDN.ToString() == certEntry.Certificate.SubjectDN.ToString())
                    continue;
                certPem += (certStart + pemify(Convert.ToBase64String(certEntry.Certificate.GetEncoded())) + certEnd + "\n");
            }

            logger.MethodExit();

            return (certPem, privateKeyString);
        }

        private void runRacadm(string args, bool wait=true)
        {
            logger.MethodEntry();
            logger.LogDebug($"Command {args}");

            ProcessStartInfo cmd = new ProcessStartInfo()
            {
                FileName = $"{racadmPath}\\racadm.exe",
                Arguments = $"--nocertwarn -r {IP} -u {user} -p {password} {args}",
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            Process p = Process.Start(cmd);
            string stdOut = p.StandardOutput.ReadToEnd();
            string stdErr = p.StandardError.ReadToEnd();
            int exitCode = p.ExitCode;

            if (wait)
            {
                p.WaitForExit();
            }

            logger.LogTrace($"Command output: {stdOut}");
            logger.LogTrace($"Exit Code & Error Text: {exitCode} - {stdErr}");
            logger.MethodExit();

            if (exitCode > 0)
                throw new Exception ($"Error processing command {args} - {exitCode.ToString()}: {stdErr}");
        }
    }
}
