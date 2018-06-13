using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.CLI
{
    /// <summary>
    /// Watches the system processes to make sure a process is running.
    /// </summary>
    public class Cli
    {
        public delegate void ProcessStartedEventHandler(string fileName, string args, Process process);
        public event ProcessStartedEventHandler ProcessStarted;

        public bool RestartEnabled { get; set; } = true;

        private DaemonInterface di = new DaemonInterface();
        private static Cli instance;

        public DaemonInterface Daemon
        {
            get { return di; }
        }

        public static Cli Instance
        {
            get { return instance; }
        }

        public static Cli CreateInstance()
        {
            if (instance != null)
                return instance;

            instance = new Cli();
            return instance;
        }

        public void StartDaemon()
        {
            string daemonPath = $"{Configuration.Instance.ToolsPath}nervad";

            #region Log file cycling

            string logFile = daemonPath + ".log";
            string oldLogFile = logFile + ".old";

            if (File.Exists(oldLogFile))
                File.Delete(oldLogFile);

            if (File.Exists(logFile))
                File.Move(logFile, oldLogFile);

            #endregion

            string daemonIp = Configuration.Instance.Daemon.PrivateRpc ? "127.0.0.1" : "0.0.0.0";
            string daemonArgs = $"--rpc-bind-ip {daemonIp} --rpc-bind-port {Configuration.Instance.Daemon.RpcPort} --log-file {logFile}";

            StartWatcherThread(daemonPath, daemonArgs);
        }

        private void StartWatcherThread(string processName, string args)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                StartProcess(processName, args);
            };

            worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (RestartEnabled)
                {
                    Log.Instance.Write(Log_Severity.Warning, "CLI tool {0} exited unexpectedly. Restarting", processName);
                    worker.RunWorkerAsync();
                }
                else
                    Log.Instance.Write(Log_Severity.Warning, "CLI tool {0} exited unexpectedly. Automatic restart disabled", processName);
            };

            worker.RunWorkerAsync();
        }

        private void StartProcess(string exe, string args)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(Path.GetFileName(exe));
                Process proc;

                #region Kill existing nervad instances

                foreach (Process p in processes)
                {
                    Log.Instance.Write(Log_Severity.Warning, "Killing already running instances of {0} with id {1}", p.ProcessName, p.Id);

                    ProcessStartInfo kpsi = new ProcessStartInfo(exe, "stop_daemon");
                    kpsi.CreateNoWindow = true;
                    kpsi.UseShellExecute = false;
                    Process kp = Process.Start(kpsi);
                    kp.WaitForExit();

                    p.WaitForExit();

                    Log.Instance.Write(Log_Severity.Info, "Kill code exited with status: {0}", kp.ExitCode);
                }

                #endregion

                if (Configuration.Instance.Daemon.AutoStartMining)
                {
                    args += $" --start-mining {Configuration.Instance.WalletAddress} --mining-threads {Configuration.Instance.Daemon.MiningThreads}";
                    Log.Instance.Write("Enabling startup mining @ {0}", Configuration.Instance.WalletAddress);
                }

                ProcessStartInfo psi = new ProcessStartInfo(exe, args);
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                Log.Instance.Write("Starting nervad {0}", args);

                proc = new Process();
                proc.StartInfo = psi;

                proc.Start();
                Thread.Sleep(5000);
                ProcessStarted?.Invoke(exe, args, proc);

                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }
        }
    }
}