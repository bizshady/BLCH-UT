using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.CLI
{
    public class DaemonCliTool : CliTool<DaemonInterface>
    {
        private string cliExe = FileNames.GetCliExeName(FileNames.NERVAD);
        public override string Exe => cliExe;

        public override string BaseExeName => FileNames.NERVAD;

        public DaemonCliTool(Cli controller) : base(controller, new DaemonInterface()) { }

        public override void Create(string exe, string args)
        {
            Log.Instance.Write("Starting process {0} {1}", exe, args);

            switch (OS.Type)
            {
                case OS_Type.Linux:
                {
                    //On Linux we have to use the --detach option to keep
                    //the daemon running after the wallet closes
                    //Using --detach spawns the daemon in a new process, different to the one we originally
                    //spawned on the next line. So we have to wait for that first one to exit, then
                    //do a search for the new nervad process and link to that. 

                    Process proc = Process.Start(new ProcessStartInfo(exe, args)
                    {
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });

                    proc.WaitForExit();
        
                    string n = Path.GetFileNameWithoutExtension(exe);
                    var p = Process.GetProcessesByName(n);
        
                    if (p.Length == 1)
                    { 
                        pid = p[0].Id;
                        controller.DoProcessStarted(exe, p[0]);
                    }
                    else
                    {
                        pid = -1;
                        Log.Instance.Write(Log_Severity.Fatal, "Error creating CLI process {0}", exe);
                    }
                }
                break;
                case OS_Type.Windows:
                {
                    //The --detach option is not available on Windows. So we just start the daemon.
                    Process proc = Process.Start(new ProcessStartInfo(exe, args)
                    {
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });
                    
                    pid = proc.Id;
                    controller.DoProcessStarted(exe, proc);
                }
                break;
            }
        }

        public override string GenerateCommandLine()
        {
            string a = GetBaseCommandLine(Exe, Configuration.Instance.Daemon.Rpc);

            if (Configuration.Instance.Daemon.AutoStartMining)
            {
                string ma = Configuration.Instance.Daemon.MiningAddress;

                Log.Instance.Write("Enabling startup mining @ {0}", ma);
                a += $" --start-mining {ma} --mining-threads {Configuration.Instance.Daemon.MiningThreads}";
            }
            
            if (OS.Type == OS_Type.Linux)
                a += " --detach";

            return a;
        }

        public override void ManageCliProcess()
        {
            bool reconnect = Configuration.Instance.ReconnectToDaemonProcess;
            bool createNew = Configuration.Instance.NewDaemonOnStartup;

            controller.ManageCliProcesses(cliExe, reconnect, ref createNew, ref pid);

            Configuration.Instance.NewDaemonOnStartup = createNew;
        }
    }
}