using System;
using System.Diagnostics;

namespace Keyfactor.Extensions.Orchestrator.IDRAC
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
            Process p = Process.Start(cmd);
            if (wait)
            {
                p.WaitForExit();
            }
        }
    }
}
