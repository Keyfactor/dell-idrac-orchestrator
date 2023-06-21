using Keyfactor.Extensions.Orchestrator.IDRAC;
using Keyfactor.Orchestrators.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            JobResult r = new Inventory().ProcessJob(new InventoryJobConfiguration()
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
            */
            string contents = Convert.ToBase64String(File.ReadAllBytes("C:\\temp\\jWdJ2UubSZ5k.pfx"));
            Console.WriteLine(contents);
            JobResult r = new Management().ProcessJob(new ManagementJobConfiguration()
            {
                OperationType = Keyfactor.Orchestrators.Common.Enums.CertStoreOperationType.Add,
                CertificateStoreDetails = new CertificateStore()
                {
                    StorePath = "C:\\Program Files (x86)\\Dell\\SysMgt\\rac5",
                    ClientMachine = "10.110.0.25"
                },
                ServerUsername = "root",
                ServerPassword = "",
                JobCertificate = new ManagementJobCertificate()
                {
                    Contents = contents,
                    PrivateKeyPassword = ""
                }
            });

            Console.WriteLine(r.FailureMessage);
        }
    }
}
