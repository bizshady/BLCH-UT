using System.Collections.Generic;
using System.Diagnostics;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.CLI
{
    public class CliInterface
    {
        protected NetHelper netHelper;

        public CliInterface(RpcDetails r)
        {
            netHelper = new NetHelper(r);
        }

        public static List<Process> GetRunningProcesses(string exe)
        {
            List<Process> r = new  List<Process>();
            var pl = Process.GetProcessesByName(exe);

            foreach (var p in pl)
                if (!p.HasExited)
                    r.Add(p);
            
            return r;
        }
    }
}