using Keyfactor.Extensions.Orchestrator.SampleOrchestratorExtension;
using Keyfactor.Orchestrators.Extensions;

using System;
using System.Linq;

namespace TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Inventory i = new Inventory();
            JobResult r = i.ProcessJob(new InventoryJobConfiguration()
            {
                CertificateStoreDetails = new CertificateStore()
                {
                    ClientMachine = "10.110.0.25"
                },
                ServerUsername = "admjd",
                ServerPassword = "DevTrust2o22!"
            },
            inventory =>
            {
                Console.WriteLine(inventory.Count());
                return true;
            }
            );
            Console.WriteLine(r.FailureMessage);
        }
    }
}
