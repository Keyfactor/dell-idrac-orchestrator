// Copyright 2024 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.IDRAC
{
    public class Inventory : IInventoryJobExtension
    {
        public string ExtensionName => "Keyfactor.Extensions.Orchestrator.IDRAC.Inventory";

        //Job Entry Point
        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());

            logger.MethodEntry();
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: {config.CertificateStoreDetails.ClientMachine}");
            logger.LogDebug($"Store Path: {config.CertificateStoreDetails.StorePath}");

            try
            {
                string racadmPath = config.CertificateStoreDetails.StorePath;
                string IP = config.CertificateStoreDetails.ClientMachine;
                string user = config.ServerUsername;
                string password = config.ServerPassword;

                IdracClient client = new IdracClient(racadmPath, IP, user, password);

                // IDRAC supports multiple certificate types.  Only certificate type of 1 (server certificate) is supported here
                List<CurrentInventoryItem> inventoryItems = client.GetCerts(1).Where(x => !(x is null)).ToList();
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
            finally
            {
                logger.MethodExit();
            }
        }
    }
}