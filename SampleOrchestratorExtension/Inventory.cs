using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.SampleOrchestratorExtension
{
    // The Inventory class implementes IAgentJobExtension and is meant to find all of the certificates in a given certificate store on a given server
    //  and return those certificates back to Keyfactor for storing in its database.  Private keys will NOT be passed back to Keyfactor Command 
    public class Inventory : IInventoryJobExtension
    {
        //Necessary to implement IInventoryJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";

        public string racadmPath;
        public string IP;
        public string user;
        public string password;
        ILogger logger;

        public void runRacadm(string args)
        {
            ProcessStartInfo cmd = new ProcessStartInfo()
            {
                FileName = $"{racadmPath}\\racadm.exe",
                Arguments = $"-r {IP} -u {user} -p {password} {args}",
                CreateNoWindow = false,
                RedirectStandardOutput = false
            };
            Console.WriteLine(cmd.FileName);
            Console.WriteLine(cmd.Arguments);
            Process p = Process.Start(cmd);
            p.WaitForExit();
        }

        // RACADM takes a "type" parameter with values 1-5 (server cert, trust root, etc) and outputs to a file.
        public List<CurrentInventoryItem> getCert(int i)
        {
            runRacadm($"sslcertdownload -t {i} -f \"cert-{i}.txt\"");
            try
            {
                List<string> fileContents = System.IO.File.ReadAllLines($"cert-{i}.txt").ToList();
                fileContents.RemoveAll(l => l.StartsWith("#"));
                string[] certs = String.Join('\n',fileContents).Split("-----BEGIN CERTIFICATE-----", StringSplitOptions.RemoveEmptyEntries).Select(x => "-----BEGIN CERTIFICATE-----"+x).ToArray();
                return certs.Select( c => new CurrentInventoryItem()
                {
                    Alias = i.ToString(),
                    Certificates = new List<string>() { c},
                    PrivateKeyEntry = false
                }).ToList();
            } catch (Exception e)
            {
                logger.LogDebug(e.Message);
                logger.LogTrace(e.StackTrace);
                return null;
            }
        }

        //Job Entry Point
        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin Inventory...");

            try
            {
                racadmPath = config.CertificateStoreDetails.StorePath;
                IP = config.CertificateStoreDetails.ClientMachine;
                user = config.ServerUsername;
                password = config.ServerPassword;
                List<CurrentInventoryItem> inventoryItems = Enumerable.Range(1, 5).Select(i => getCert(i)).Where(x => !(x is null)).SelectMany(x => x).ToList();
                submitInventory.Invoke(inventoryItems);
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = ex.Message };
            }            
        }
    }
}