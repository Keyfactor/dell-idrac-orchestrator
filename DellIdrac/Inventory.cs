using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.IDRAC
{
    // The Inventory class implementes IAgentJobExtension and is meant to find all of the certificates in a given certificate store on a given server
    //  and return those certificates back to Keyfactor for storing in its database.  Private keys will NOT be passed back to Keyfactor Command 
    public class Inventory : IInventoryJobExtension
    {
        //Necessary to implement IInventoryJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";
        ILogger logger;
        IdracClient client;

        // RACADM takes a "type" parameter with values 1-10 (server cert, trust root, etc) and outputs to a file.
        public List<CurrentInventoryItem> getCert(int i)
        {
            client.runRacadm($"sslcertdownload -t {i} -f \"{client.racadmPath}{Path.DirectorySeparatorChar}cert-{i}.txt\"");
            try
            {
                List<string> fileContents = System.IO.File.ReadAllLines($"{client.racadmPath}{Path.DirectorySeparatorChar}cert-{i}.txt").ToList();
                fileContents.RemoveAll(l => l.StartsWith("#"));
                string[] certs = string.Join('\n',fileContents).Split("-----BEGIN CERTIFICATE-----", StringSplitOptions.RemoveEmptyEntries).Select(x => "-----BEGIN CERTIFICATE-----"+x).ToArray();
                return certs.Select( c => new CurrentInventoryItem()
                {
                    Alias = $"{i}.{Array.IndexOf(certs,c)}",
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
                string racadmPath = config.CertificateStoreDetails.StorePath;
                string IP = config.CertificateStoreDetails.ClientMachine;
                string user = config.ServerUsername;
                string password = config.ServerPassword;
                client = new IdracClient(racadmPath, IP, user, password);
                // IDRAC supports ~10 cert types identified by number. Take union of certs across all types.
                List<CurrentInventoryItem> inventoryItems = Enumerable.Range(1, 10).Select(i => getCert(i)).Where(x => !(x is null)).SelectMany(x => x).ToList();
                submitInventory.Invoke(inventoryItems);
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message);
                logger.LogTrace(ex.StackTrace);
                return new JobResult() { 
                    Result = Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId, 
                    FailureMessage = ex.Message 
                };
            }            
        }
    }
}