using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DellIDRACOrchestrator
{
    public class IdracClient
    {
        public string racadmPath;
        public string IP;
        public string user;
        public string password;

        public IdracClient(string racadmPath, string IP, string user, string password)
        {
            this.racadmPath = racadmPath;
            this.IP = IP;
            this.user = user;
            this.password = password;
        }

        public void runRacadm(string args, bool wait=true)
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
            if (wait)
            {
                p.WaitForExit();
            }
        }
    }
}
