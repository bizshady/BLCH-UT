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
    public class WalletCliTool : CliTool<WalletInterface>
    {
        private string cliExe = FileNames.GetCliExeName(FileNames.RPC_WALLET);
        public override string Exe => cliExe;

        public override string BaseExeName => FileNames.RPC_WALLET;

        public WalletCliTool(Cli controller) : base(controller, new WalletInterface()) { }

        public override void Create(string exe, string args)
        {
            Log.Instance.Write("Starting process {0} {1}", exe, args);

            Process proc = Process.Start(new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            pid = proc.Id;
            controller.DoProcessStarted(exe,  proc);
            proc.WaitForExit();
        }

        public override string GenerateCommandLine()
        {
            string a = GetBaseCommandLine(Exe, Configuration.Instance.Wallet.Rpc);
            a += $" --disable-rpc-login --wallet-dir {Configuration.Instance.Wallet.WalletDir}";
            a += $" --daemon-address 127.0.0.1:{Configuration.Instance.Daemon.Rpc.Port}";
            return a;
        }

        public override void ManageCliProcess()
        {
            bool createNew = true;
            controller.ManageCliProcesses(cliExe, false, ref createNew, ref pid);
        }
    }
}