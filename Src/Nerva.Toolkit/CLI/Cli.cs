using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nerva.Toolkit.CLI
{
    /// <summary>
    /// Watches the system processes to make sure a process is running.
    /// </summary>
    public class Cli
    {
        public delegate void ProcessStartedEventHandler(string fileName, string args, Process process);
        public event ProcessStartedEventHandler ProcessStarted, ProcessConnected;

        private int dPid = -1, walletPid = -1;
        private bool doDaemonCrashCheck = true, doWalletCrashCheck = true;
        private BackgroundWorker daemonWorker, walletWorker;
        private DaemonInterface di = new DaemonInterface();
        private WalletInterface wi = new WalletInterface();

        private static Cli instance;

        public DaemonInterface Daemon => di;

        public WalletInterface Wallet => wi;

        public static Cli Instance => instance;

        public static Cli CreateInstance()
        {
            if (instance != null)
                return instance;

            instance = new Cli();
            return instance;
        }

        public string CycleLogFile(string path)
        {
            string logFile = path + ".log";
            string oldLogFile = logFile + ".old";

            if (File.Exists(oldLogFile))
                File.Delete(oldLogFile);

            if (File.Exists(logFile))
                File.Move(logFile, oldLogFile);

            return logFile;
        }

        /*public string GetDaemonCommandLine(string logFile)
        {
            string daemonArgs = $"--rpc-bind-port {Configuration.Instance.Daemon.RpcPort} --log-file {logFile} --detach";

            if (!Configuration.Instance.Daemon.PrivateRpc)
            {
                string user = Configuration.Instance.Daemon.RpcLogin;
                string pass = Configuration.Instance.Daemon.RpcPass;

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    Log.Instance.Write(Log_Severity.Error, "RPC username or password not set. Public RPC access is disabled");
                    daemonArgs += $" --rpc-bind-ip 127.0.0.1";
                }
                else
                {
                    daemonArgs += $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind";
                    daemonArgs += $" --rpc-login {user}:{pass}";
                }
            }

            if (Configuration.Instance.Daemon.AutoStartMining)
            {
                Log.Instance.Write("Enabling startup mining @ {0}", Configuration.Instance.WalletAddress);
                daemonArgs += $" --start-mining {Configuration.Instance.WalletAddress} --mining-threads {Configuration.Instance.Daemon.MiningThreads}";
            }

            if (Configuration.Instance.Testnet)
            {
                Log.Instance.Write("Connecting to testnet");
                daemonArgs += " --testnet";
            }

            return daemonArgs;
        }*/

        public void StartDaemon()
        {
            string exeName = (Environment.OSVersion.Platform == PlatformID.Win32NT) ? "nervad.exe" : "nervad";
            string exePath = $"{Configuration.Instance.ToolsPath}{exeName}";

            #region Build the command line

            string exeArgs = $"--rpc-bind-port {Configuration.Instance.Daemon.RpcPort} --log-file {CycleLogFile(exePath)} --detach";

            if (!Configuration.Instance.Daemon.Credentials.PrivateRpc)
            {
                string user = Configuration.Instance.Daemon.Credentials.RpcLogin;
                string pass = Configuration.Instance.Daemon.Credentials.RpcPass;

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    Log.Instance.Write(Log_Severity.Error, "RPC username or password not set. Public RPC access is disabled");
                    exeArgs += $" --rpc-bind-ip 127.0.0.1";
                }
                else
                {
                    exeArgs += $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind";
                    exeArgs += $" --rpc-login {user}:{pass}";
                }
            }

            if (Configuration.Instance.Daemon.AutoStartMining)
            {
                Log.Instance.Write("Enabling startup mining @ {0}", Configuration.Instance.WalletAddress);
                exeArgs += $" --start-mining {Configuration.Instance.WalletAddress} --mining-threads {Configuration.Instance.Daemon.MiningThreads}";
            }

            if (Configuration.Instance.Testnet)
            {
                Log.Instance.Write("Connecting to testnet");
                exeArgs += " --testnet";
            }

            #endregion

            #region Create BackgroundWorker that will do the crash checking

            daemonWorker = new BackgroundWorker();
            daemonWorker.WorkerSupportsCancellation = true;

            daemonWorker.DoWork += (sender, e) =>
            {
                if (doDaemonCrashCheck)
                {
                    StartDaemonProcessMonitor(exePath, exeArgs);
                    Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);
                }
            };

            daemonWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                { //Been cancelled. Return so the worker is not run again
                    Log.Instance.Write(Log_Severity.Warning, "Daemon crash check has been cancelled");
                    return;
                }

                if (doDaemonCrashCheck)
                    daemonWorker.RunWorkerAsync();
            };

            #endregion
            
            //Start crash checking
            daemonWorker.RunWorkerAsync();
        }

        public void StartWallet()
        {
            string exeName = (Environment.OSVersion.Platform == PlatformID.Win32NT) ? "nerva-wallet-rpc.exe" : "nerva-wallet-rpc";
            string exePath = $"{Configuration.Instance.ToolsPath}{exeName}";
            //string daemonArgs = GetDaemonCommandLine(CycleLogFile(daemonPath));

            #region Build the command line

            string exeArgs = $"--rpc-bind-port {Configuration.Instance.Wallet.RpcPort} --log-file {CycleLogFile(exePath)} --detach";

            if (!Configuration.Instance.Wallet.Credentials.PrivateRpc)
            {
                string user = Configuration.Instance.Daemon.Credentials.RpcLogin;
                string pass = Configuration.Instance.Daemon.Credentials.RpcPass;

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    Log.Instance.Write(Log_Severity.Error, "RPC username or password not set. Public RPC access is disabled");
                    exeArgs += $" --rpc-bind-ip 127.0.0.1";
                }
                else
                {
                    exeArgs += $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind";
                    exeArgs += $" --rpc-login {user}:{pass}";
                }
            }

            if (Configuration.Instance.Testnet)
            {
                Log.Instance.Write("Connecting to testnet");
                exeArgs += " --testnet";
            }

            #endregion
        }

        public bool CheckIsAlreadyRunning(string exe, ref int pid)
        {
            if (pid != -1)
            {
                try
                {
                    Process dp = Process.GetProcessById(pid);
                    if (dp == null || dp.HasExited)
                    {
                        Log.Instance.Write(Log_Severity.Warning, "CLI tool {0} exited unexpectedly. Restarting", exe);
                        pid = -1;
                        return false;
                    }
                    else
                        return true;
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteNonFatalException(ex);
                    pid = -1;
                    return false;
                }
            }

            return false;
        }

        public void ManageExistingProcesses(string exe, string args, bool reconnect, ref bool createNew, ref int pid)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileName(exe));

            if (processes.Length > 0)
            {
                //Reconnect to an existing process if only one is running
                //If more than 1, we have to start again, because we cannot be sure which one to connec to
                if (processes.Length == 1 && reconnect && !createNew)
                {
                    Process p = processes[0];
                    pid = p.Id;
                    ProcessConnected?.Invoke(exe, args, p);
                }
                else
                {
                    #region Kill existing instances

                    foreach (Process p in processes)
                        if (p.Id != pid)
                        {
                            Log.Instance.Write(Log_Severity.Warning, "Killing running instance of {0} with id {1}", p.ProcessName, p.Id);
                            p.Kill();
                            p.WaitForExit();
                        }

                    processes = Process.GetProcessesByName(Path.GetFileName(exe));

                    if (processes.Length > 0)
                    {
                        Log.Instance.Write(Log_Severity.Fatal, "There are unknown daemon proceses running. Please kill all processes");
                        return;
                    }

                    pid = -1;
                                
                    #endregion
                }

                createNew = false;
            }
        }

        public void CreateNewProcess(string exe, string args, ref int pid)
        {
            ProcessStartInfo psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process proc = new Process
            {
                StartInfo = psi
            };

            Log.Instance.Write("Starting process {0} {1}", exe, args);

            proc.Start();
            proc.WaitForExit();

            ProcessStarted?.Invoke(exe, args, proc);

            //We can assume there is only one nervad process running by now.
            pid = Process.GetProcessesByName(Path.GetFileName(exe))[0].Id;
        }

        private void StartDaemonProcessMonitor(string exe, string args)
        {
            try
            {
                if (CheckIsAlreadyRunning(exe, ref dPid))
                    return;

                bool createNew = Configuration.Instance.NewDaemonOnStartup;

                ManageExistingProcesses(exe, args, Configuration.Instance.Daemon.ReconnectToDaemonProcess, 
                    ref createNew, ref dPid);

                Configuration.Instance.NewDaemonOnStartup = createNew;

                if (dPid == -1)
                    CreateNewProcess(exe, args, ref dPid);
                else
                    Log.Instance.Write("Connecting to process {0} with id {1}", exe, dPid);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }
        }

        public void StopDaemonCheck()
        {
            dPid = -1;
            doDaemonCrashCheck = false;
            daemonWorker.CancelAsync();
        }
    }
}