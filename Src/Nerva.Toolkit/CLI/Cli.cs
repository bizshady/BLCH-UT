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
using AngryWasp.Helpers;
using Nerva.Toolkit.Content.Dialogs;
using Eto.Forms;

namespace Nerva.Toolkit.CLI
{
    /// <summary>
    /// Watches the system processes to make sure a process is running.
    /// </summary>
    public class Cli
    {
        public delegate void ProcessStartedEventHandler(string fileName, string args, Process process);
        public event ProcessStartedEventHandler ProcessStarted, ProcessEnded, ProcessConnected;

        private int daemonPid = -1, walletPid = -1;
        private bool doDaemonCrashCheck = true, doWalletCrashCheck = true;
        private BackgroundWorker daemonWorker, walletWorker;
        private DaemonInterface di = new DaemonInterface();
        private WalletInterface wi = new WalletInterface();

        private static Cli instance;

        public DaemonInterface Daemon => di;

        public WalletInterface Wallet => wi;

        public int DaemonPid => daemonPid;

        public int WalletPid => walletPid;

        public static Cli Instance => instance;

        public bool CliProcessIsReady(int pid)
        {
            if (pid == -1)
                return false;

            Process dp = Process.GetProcessById(pid);
            if (dp == null || dp.HasExited)
                return false;

            double ms = (DateTime.Now - dp.StartTime).TotalMilliseconds;
            if (ms < Constants.DAEMON_RESTART_THREAD_INTERVAL)
                return false;

            return true;
        }

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

        private string GetBaseCommandLine(string exe, out string formattedExePath)
        {
            string exeName = (Environment.OSVersion.Platform == PlatformID.Win32NT) ? $"{exe}.exe" : exe;
            formattedExePath = $"{Configuration.Instance.ToolsPath}{exeName}";

            string arg = $"--log-file {CycleLogFile(formattedExePath)}";

            if (Configuration.Instance.Testnet)
            {
                Log.Instance.Write("Connecting to testnet");
                arg += " --testnet";
            }

            return arg;
        }

        private string GetRpcBindCommandLine(RpcDetails d)
        {
            string arg = $" --rpc-bind-port {d.Port}";

            //TODO: How the fuck does .NET handle Digest authentication? Uncomment to enable rpc user:pass
            //after that is worked out 
            /*if (string.IsNullOrEmpty(d.Login))
            {
                Log.Instance.Write(Log_Severity.Error, "RPC username not set. Generating a new random user name");
                d.Login = StringHelper.GenerateRandomString(24);
            }

            if (string.IsNullOrEmpty(d.Pass))
            {
                Log.Instance.Write(Log_Severity.Error, "RPC password not set. Generating a new random user name");
                d.Pass = StringHelper.GenerateRandomString(24);
            }
            
            string ip = d.IsPublic ? $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind" : $" --rpc-bind-ip 127.0.0.1";

            arg += $" --rpc-login {d.Login}:{d.Pass}";*/

            return arg;
        }

        public void Start()
        {
            ProcessStarted += DoCliStartup;
            ProcessConnected += DoCliStartup;

            StartDaemon();
            StartWallet();
        }

        public void StartDaemon()
        {
            string exePath;

            string arg = GetBaseCommandLine("nervad", out exePath);
            arg += GetRpcBindCommandLine(Configuration.Instance.Daemon.Rpc);

            if (Configuration.Instance.Daemon.AutoStartMining)
            {
                string ma = Configuration.Instance.Daemon.MiningAddress;

                Log.Instance.Write("Enabling startup mining @ {0}", ma);
                arg += $" --start-mining {ma} --mining-threads {Configuration.Instance.Daemon.MiningThreads}";
            }

            arg += " --detach";

            #region Create BackgroundWorker that will do the crash checking

            daemonWorker = new BackgroundWorker();
            daemonWorker.WorkerSupportsCancellation = true;

            daemonWorker.DoWork += (sender, e) =>
            {
                if (doDaemonCrashCheck)
                {
                    try
                    {
                        if (CheckIsAlreadyRunning(exePath, ref daemonPid))
                            return;

                        bool reconnect = Configuration.Instance.ReconnectToDaemonProcess;
                        bool createNew = Configuration.Instance.NewDaemonOnStartup;

                        ManageExistingProcesses(exePath, arg, reconnect, ref createNew, ref daemonPid);

                        Configuration.Instance.NewDaemonOnStartup = createNew;

                        if (daemonPid == -1)
                            CreateNewProcess(exePath, arg, ref daemonPid);
                        else
                            Log.Instance.Write("Connecting to process {0} with id {1}", exePath, daemonPid);
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.WriteFatalException(ex);
                    }

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
            string wd = Configuration.Instance.Wallet.WalletDir;

            //Make sure the wallet dir exists
            if (!Directory.Exists(wd))
            {
                Log.Instance.Write("Creating wallet directory @ {0}", wd);
                Directory.CreateDirectory(wd);
            }
            else
            {
                //TODO: Load wallet file paths
                Log.Instance.Write("Loading wallets from {0}", wd);
            }

            string exePath;

            string arg = GetBaseCommandLine("nerva-wallet-rpc", out exePath);
            arg += GetRpcBindCommandLine(Configuration.Instance.Wallet.Rpc);

            arg += $" --disable-rpc-login --wallet-dir {Configuration.Instance.Wallet.WalletDir}";

            #region Create BackgroundWorker that will do the crash checking

            walletWorker = new BackgroundWorker();
            walletWorker.WorkerSupportsCancellation = true;

            walletWorker.DoWork += (sender, e) =>
            {
                if (doWalletCrashCheck)
                {
                    try
                    {
                        if (CheckIsAlreadyRunning(exePath, ref walletPid))
                            return;

                        bool reconnect = Configuration.Instance.ReconnectToWalletProcess;
                        bool createNew = Configuration.Instance.NewWalletOnStartup;

                        ManageExistingProcesses(exePath, arg, reconnect, ref createNew, ref walletPid);

                        Configuration.Instance.NewWalletOnStartup = createNew;

                        if (walletPid == -1)
                            CreateNewProcess(exePath, arg, ref walletPid);
                        else
                            Log.Instance.Write("Connecting to process {0} with id {1}", exePath, walletPid);
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.WriteFatalException(ex);
                    }

                    Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);
                }
            };

            walletWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {
                    Log.Instance.Write(Log_Severity.Warning, "Wallet crash check has been cancelled");
                    return;
                }

                if (doWalletCrashCheck)
                    walletWorker.RunWorkerAsync();
            };

            #endregion

            //Start crash checking
            walletWorker.RunWorkerAsync();
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

            string n = Path.GetFileName(exe);
            var p = Process.GetProcessesByName(n);

            if (p.Length == 1)
            {
                pid = p[0].Id;
                ProcessStarted?.Invoke(exe, args, proc);
            }
            else
                Log.Instance.Write(Log_Severity.Fatal, "Error creating CLI process {0}", exe);
        }

        public void StopDaemonCheck()
        {
            daemonPid = -1;
            doDaemonCrashCheck = false;
            daemonWorker.CancelAsync();
        }

        public void StopWalletCheck()
        {
            walletPid = -1;
            doWalletCrashCheck = false;
            walletWorker.CancelAsync();
        }

        #region Startup events

        private void DoCliStartup(string exe, string arg, Process process)
        {
            string exeName = Path.GetFileNameWithoutExtension(exe).ToLower();
            if (exeName == "nervad")
                UpdateCheck();
            else if (exeName == "nerva-wallet-rpc")
                LoadWallet();
            else
                Log.Instance.Write(Log_Severity.Error, "Unknown process connected '{0}'", exeName);
        }

        private void UpdateCheck()
        {
            BackgroundWorker w = new BackgroundWorker();

            w.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                while (!CliProcessIsReady(daemonPid))
                    Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);

                UpdateManager.CheckForCliUpdates();

                switch (UpdateManager.UpdateStatus)
                {
                    case Update_Status_Code.UpToDate:
                            Log.Instance.Write("NERVA CLI tools are up to date");
                        break;
                    case Update_Status_Code.NewVersionAvailable:
                            Log.Instance.Write("A new version of the NERVA CLI tools are available");
                        break;
                    default:
                            Log.Instance.Write(Log_Severity.Error, "An error occurred checking for updates.");
                        break;
                }

                Log.Instance.Write("Update check complete");
            };

            if (Configuration.Instance.CheckForUpdateOnStartup)
                w.RunWorkerAsync();
        }

        private void LoadWallet()
        {
            BackgroundWorker w = new BackgroundWorker();

            w.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                while (!CliProcessIsReady(walletPid))
                    Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);

                Application.Instance.AsyncInvoke ( () =>
				{
					if (!WalletHelper.OpenSavedWallet())
                        WalletHelper.ShowWalletWizard();
				});
            };

            w.RunWorkerAsync();
        }

        #endregion
    }
}