using System.Collections.Generic;
using System.Diagnostics;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.CLI
{
    public class CliInterface
    {
        protected RpcDetails r;

        public CliInterface(RpcDetails r)
        {
            this.r = r;
        }

        public static List<Process> GetRunningProcesses(string exe)
        {
            Process[] pl = Process.GetProcessesByName(exe);
            List<Process> r = new  List<Process>();

            foreach (var p in pl)
            {
                if (p.HasExited)
                    continue;
                    
                r.Add(p);
            }
            
            return r;
        }
    }
}