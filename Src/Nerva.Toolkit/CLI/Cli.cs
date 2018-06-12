using System;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.CLI
{
    /// <summary>
    /// Watches the system processes to make sure a process is running.
    /// </summary>
    public static class CliInterface
    {
        private static DaemonInterface daemon;
        private static WalletInterface wallet;

        public static DaemonInterface DaemonConfig
        {
            get { return daemon; }
        }

        public static void Start()
        {
            daemon = new DaemonInterface();
            wallet = new WalletInterface();

            ProcessWatcher watcher = new ProcessWatcher();

            string daemonPath = $"{Configuration.Instance.ToolsPath}nervad";
            string daemonIp = Configuration.Instance.DaemonConfig.PrivateRpc ? "127.0.0.1" : "0.0.0.0";
            string daemonArgs = $"--rpc-bind-ip {daemonIp} --rpc-bind-port {Configuration.Instance.DaemonConfig.RpcPort}";

            watcher.StartWatcherThread(daemonPath, daemonArgs);
        }
    }
}