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
    public abstract class CliTool<T> where T : CliInterface, new()
    {
        protected int pid = -1;
        protected bool doCrashCheck = true;
        protected BackgroundWorker worker = null;
        protected T cliInterface;
        protected Cli controller = null;

        public int Pid => pid;

        public T Interface => cliInterface;

        public abstract string Exe { get; }

        public abstract string BaseExeName { get; }

        public CliTool(Cli controller, T cliInterface)
        {
            this.controller = controller;
            this.cliInterface = cliInterface;
        }

        protected string GetBaseCommandLine(string exe, RpcDetails d)
        {
            string e = FileNames.GetCliExePath(exe);

            string arg = $"--log-file {controller.CycleLogFile(e)}";

            if (Configuration.Instance.Testnet)
            {
                Log.Instance.Write("Connecting to testnet");
                arg += " --testnet";
            }

            arg += $" --rpc-bind-port {d.Port}";

            // TODO: Uncomment to enable rpc user:pass.
            // string ip = d.IsPublic ? $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind" : $" --rpc-bind-ip 127.0.0.1";
            // arg += $"{ip} --rpc-login {d.Login}:{d.Pass}";

            return arg;
        }

        public virtual void StartCrashCheck()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += (sender, e) =>
            {
                while (doCrashCheck)
                {
                    try
                    {
                        string ex = FileNames.GetCliExePath(Exe);

                        if (!controller.IsAlreadyRunning(ex, ref pid))
                        {
                            ManageCliProcess();

                            if (pid == -1)
                                Create(ex, GenerateCommandLine());
                            else
                                Log.Instance.Write("Connecting to process {0} with id {1}", ex, pid);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.WriteFatalException(ex);
                    }

                    Thread.Sleep(Constants.ONE_SECOND);
                }
            };

            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                { //Been cancelled. Return so the worker is not run again
                    Log.Instance.Write(Log_Severity.Warning, "Daemon crash check has been cancelled");
                    return;
                }

                if (doCrashCheck)
                    worker.RunWorkerAsync();
            };

            //Start crash checking
            worker.RunWorkerAsync();
        }

        public virtual void ResumeCrashCheck()
        {
            pid = -1;
            doCrashCheck = true;

            if (worker != null)
                worker.RunWorkerAsync();
            else
                StartCrashCheck();
        }

        public virtual void StopCrashCheck()
        {
            pid = -1;
            doCrashCheck = false;

            if (worker != null)
                worker.CancelAsync();
        }

        public virtual void ForceClose()
        {
            controller.KillCliProcesses(BaseExeName);
            pid = -1;
        }

        public abstract void Create(string exe, string args);
        public abstract void ManageCliProcess();
        public abstract string GenerateCommandLine();
    }

    public class Cli
    {
        public delegate void ProcessStartedEventHandler(string fileName, Process process);
        public event ProcessStartedEventHandler ProcessStarted, ProcessConnected;

        private static Cli instance;

        public static Cli Instance
        {
            get
            {
                if (instance == null)
                    instance = new Cli();

                return instance;
            }
        }

        private DaemonCliTool daemon;
        private WalletCliTool wallet;

        public DaemonCliTool Daemon => daemon;

        public WalletCliTool Wallet => wallet;

        public void StartDaemon()
        {
            daemon = new DaemonCliTool(this);
            daemon.StartCrashCheck();
        }

        public void StartWallet()
        {
            wallet = new WalletCliTool(this);
            wallet.StartCrashCheck();
        }

        public string CycleLogFile(string path)
        {
            string logFile = path + ".log";
            string oldLogFile = logFile + ".old";

            try
            {
                if (File.Exists(oldLogFile))
                    File.Delete(oldLogFile);

                if (File.Exists(logFile))
                    File.Move(logFile, oldLogFile);
            }
            catch (Exception)
            {
                logFile = FileHelper.RenameDuplicateFile(logFile);
                Log.Instance.Write(Log_Severity.Warning, "Cannot cycle log file. New log will be written to {0}", logFile);
                return logFile;
            }

            return logFile;
        }

        public bool IsReady(int pid)
        {
            if (pid == -1)
                return false;

            Process dp = Process.GetProcessById(pid);
            if (dp == null || dp.HasExited)
                return false;

            double ms = (DateTime.Now - dp.StartTime).TotalMilliseconds;
            if (ms < Constants.FIVE_SECONDS)
                return false;

            return true;
        }

        public bool IsAlreadyRunning(string exe, ref int pid)
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
                    Log.Instance.Write(Log_Severity.Warning, ex.Message);
                    pid = -1;
                    return false;
                }
            }

            return false;
        }

        public void ManageCliProcesses(string exe, bool reconnect, ref bool createNew, ref int pid)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe));

            if (processes.Length > 0)
            {
                //Reconnect to an existing process if only one is running
                //If more than 1, we have to start again, because we cannot be sure which one to connec to
                if (processes.Length == 1 && reconnect && !createNew)
                {
                    Process p = processes[0];
                    pid = p.Id;
                    ProcessConnected?.Invoke(exe, p);
                }
                else
                {
                    KillCliProcesses(exe, pid);
                    pid = -1;
                }

                createNew = false;
            }
        }

        public void KillCliProcesses(string exe, int exceptPid = -1)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe));

            if (processes.Length > 0)
            {
                foreach (Process p in processes)
                {
                    if (p.Id != exceptPid)
                    {
                        Log.Instance.Write(Log_Severity.Warning, "Killing running instance of {0} with id {1}", p.ProcessName, p.Id);
                        p.Kill();
                        p.WaitForExit();
                    }
                }

                processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe));

                if (processes.Length > 0)
                {
                    Log.Instance.Write(Log_Severity.Fatal, "There are unknown proceses running. Please kill all processes");
                    return;
                }
            }
            else
                Log.Instance.Write("No processes to kill");
        }

        public void DoProcessStarted(string exe, Process proc)
        {
            DoCliStartup(exe);
            ProcessStarted?.Invoke(exe, proc);
        }

        #region Startup events

        private void DoCliStartup(string exe)
        {
            switch (FileNames.GetCliExeBaseName(exe))
            {
                case FileNames.NERVAD:
                    UpdateCheck();
                    break;
                case FileNames.RPC_WALLET:
                    LoadWallet();
                    break;
                default:
                    Log.Instance.Write(Log_Severity.Error, "CLI exe file {0} is invalid", exe);
                    break;
            }
        }

        private void UpdateCheck()
        {
            BackgroundWorker w = new BackgroundWorker();

            w.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                while (!IsReady(daemon.Pid))
                    Thread.Sleep(Constants.FIVE_SECONDS);

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

            w.DoWork += (s, e) =>
            {
                while (!IsReady(wallet.Pid))
                    Thread.Sleep(Constants.FIVE_SECONDS);

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