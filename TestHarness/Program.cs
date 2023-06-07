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
            var x = "abcabc".Split("a");

            Inventory i = new Inventory();
            JobResult r = i.ProcessJob(new InventoryJobConfiguration()
            {
                CertificateStoreDetails = new CertificateStore()
                {
                    StorePath = "C:\\Program Files (x86)\\Dell\\SysMgt\\rac5",
                    ClientMachine = "10.110.0.25"
                },
                ServerUsername = "root",
                ServerPassword = ""
            },
            inventory =>
            {
                Console.WriteLine(inventory.Count());
                Console.WriteLine(String.Join("\n", inventory.Select(x => x.Certificates.First())));
                return true;
            }
            );
            Console.WriteLine(r.FailureMessage);
        }
    }
}
