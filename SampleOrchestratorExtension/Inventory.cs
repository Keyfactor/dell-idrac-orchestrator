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

        public string IP;
        public string user;
        public string password;

        public void runRacadm(string args)
        {
            ProcessStartInfo cmd = new ProcessStartInfo()
            {
                FileName = "C:\\Program Files (x86)\\Dell\\SysMgt\\rac5\\racadm.exe",
                Arguments = $"-r {IP} -u {user} -p {password} {args}",
                CreateNoWindow = false,
                RedirectStandardOutput = false
            };
            Console.WriteLine(cmd.FileName);
            Console.WriteLine(cmd.Arguments);
            Process p = Process.Start(cmd);
            p.WaitForExit();
        }

        // RACADM takes a "type" parameter with values 1-5 (server cert, trust root, etc)
        public CurrentInventoryItem getCert(int i)
        {
            runRacadm($"sslcertdownload -t {i} -f \"cert-{i}.txt\"");
            return new CurrentInventoryItem() {
                Alias = i.ToString(),
                Certificates = new List<string>(){ System.IO.File.ReadAllText($"cert-{i}.txt") },
                PrivateKeyEntry = false
             };
        }

        //Job Entry Point
        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            //NLog Logging to c:\CMS\Logs\CMS_Agent_Log.txt
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin Inventory...");

            try
            {
                IP = config.CertificateStoreDetails.ClientMachine;
                user = config.ServerUsername;
                password = config.ServerPassword;
                List<CurrentInventoryItem> inventoryItems = Enumerable.Range(1, 5).Select(i => getCert(i)).ToList();
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