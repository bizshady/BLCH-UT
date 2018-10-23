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
            //todo: GetProcessesByName may result in error. Use Process.MainModule.ModuleName
            List<Process> r = new  List<Process>();
            var pl = Process.GetProcessesByName(exe);

            foreach (var p in pl)
                if (!p.HasExited)
                    r.Add(p);
            
            return r;
        }
    }
}