// Copyright 2024 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Keyfactor.Extensions.Orchestrator.IDRAC
{
    public class Management : IManagementJobExtension
    {
        //Necessary to implement IManagementJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";
        ILogger logger;
        IdracClient client;

        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: {config.CertificateStoreDetails.ClientMachine}");
            logger.LogDebug($"Store Path: {config.CertificateStoreDetails.StorePath}");

            try
            {
                string racadmPath = config.CertificateStoreDetails.StorePath;
                string IP = config.CertificateStoreDetails.ClientMachine;
                string user = config.ServerUsername;
                string password = config.ServerPassword;
                client = new IdracClient(racadmPath, IP, user, password);

                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:
                        return AddCert(config);
                    case CertStoreOperationType.Remove:
                        throw new InvalidOperationException("'Remove' operation not supported");
                    case CertStoreOperationType.Create:
                        throw new InvalidOperationException("'Create' operation not supported");
                    default:
                        return new JobResult()
                        {
                            Result = OrchestratorJobStatusJobResult.Failure,
                            JobHistoryId = config.JobHistoryId,
                            FailureMessage = $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}: Unsupported operation: {config.OperationType.ToString()}"
                        };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                logger.LogTrace(ex.StackTrace);

                return new JobResult()
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = ex.Message
                };
            }
            finally
            {
                logger.MethodExit();
            }
        }

        private (string, string) GetPemFromPFX(string pfx, string pfxPassword)
        {
            logger.MethodEntry();

            byte[] pfxBytes = Convert.FromBase64String(pfx);
            Pkcs12Store p = new Pkcs12Store(new MemoryStream(pfxBytes), pfxPassword.ToCharArray());

            // Extract private key
            MemoryStream memoryStream = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(memoryStream);
            PemWriter pemWriter = new PemWriter(streamWriter);

            String alias = ( p.Aliases.Cast<String>() ).SingleOrDefault(a => p.IsKeyEntry(a));
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
            pemify = ( ss => ss.Length <= 64 ? ss : ss.Substring(0, 64) + "\n" + pemify(ss.Substring(64)) );

            string certPem = string.Empty;
            foreach (X509CertificateEntry certEntry in p.GetCertificateChain(alias))
            {
                if (certEntry.Certificate.IssuerDN.ToString() == certEntry.Certificate.SubjectDN.ToString())
                    continue;
                certPem += ( certStart + pemify(Convert.ToBase64String(certEntry.Certificate.GetEncoded())) + certEnd + "\n" );
            }

            logger.MethodExit();

            return (certPem, privateKeyString);
        }

        private JobResult AddCert(ManagementJobConfiguration config)
        {
            logger.MethodEntry();

            (string cert, string key) = GetPemFromPFX(config.JobCertificate.Contents, config.JobCertificate.PrivateKeyPassword);
            string salt = new Random().Next().ToString();
            
            File.WriteAllText($"{client.racadmPath}{Path.DirectorySeparatorChar}uploadkey{salt}.txt", key);
            File.WriteAllText($"{client.racadmPath}{Path.DirectorySeparatorChar}uploadcert{salt}.txt", cert);

            client.runRacadm($"sslkeyupload -t 1 -f \"{client.racadmPath}{Path.DirectorySeparatorChar}uploadkey{salt}.txt\"");
            // IDRAC automatically restarts on cert upload, which takes about 5 mins.
            client.runRacadm($"sslcertupload -t 1 -f \"{client.racadmPath}{Path.DirectorySeparatorChar}uploadcert{salt}.txt\"", false);

            File.Delete($"{client.racadmPath}{Path.DirectorySeparatorChar}uploadkey{salt}.txt");
            
            logger.MethodExit();

            return new JobResult()
            {
                Result = OrchestratorJobStatusJobResult.Success,
                JobHistoryId = config.JobHistoryId,
                FailureMessage = ""
            };
        }
    }
}