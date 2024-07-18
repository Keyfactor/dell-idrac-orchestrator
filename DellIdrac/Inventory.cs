// Copyright 2024 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.IDRAC
{
    public class Inventory : IInventoryJobExtension
    {
        public string ExtensionName => "Keyfactor.Extensions.Orchestrator.IDRAC.Inventory";
        ILogger logger;
        IdracClient client;

        //Job Entry Point
        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
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

                // IDRAC supports ~10 cert types identified by number. Take union of certs across all types.
                List<CurrentInventoryItem> inventoryItems = Enumerable.Range(1, 1).Select(i => GetCerts(i)).Where(x => !(x is null)).SelectMany(x => x).ToList();
                submitInventory.Invoke(inventoryItems);
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message);
                logger.LogTrace(ex.StackTrace);
                return new JobResult()
                {
                    Result = Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = ex.Message
                };
            }
        }

        // RACADM takes a "type" parameter with values 1-10 (server cert, trust root, etc) and outputs to a file.
        public List<CurrentInventoryItem> GetCerts(int i)
        {
            logger.MethodEntry();

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
            } 
            catch (Exception e)
            {
                logger.LogDebug(e.Message);
                logger.LogTrace(e.StackTrace);
                return null;
            }
            finally
            {
                logger.MethodExit();
            }
        }
    }
}