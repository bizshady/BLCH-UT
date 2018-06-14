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
        public event ProcessStartedEventHandler DaemonStarted;
        public event ProcessStartedEventHandler WalletStarted;

        private int daemonPid = -1;

        private Timer daemonCheckTimer = new Timer(Constants.DAEMON_RESTART_THREAD_INTERVAL);

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

        public string CycleDaemonLogFile(string daemonPath)
        {
            string logFile = daemonPath + ".log";
            string oldLogFile = logFile + ".old";

            if (File.Exists(oldLogFile))
                File.Delete(oldLogFile);

            if (File.Exists(logFile))
                File.Move(logFile, oldLogFile);

            return logFile;
        }

        public string GetDaemonCommandLine(string logFile)
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

            return daemonArgs;
        }

        public void StartDaemon()
        {
            string daemonPath = $"{Configuration.Instance.ToolsPath}nervad";
            string daemonArgs = GetDaemonCommandLine(CycleDaemonLogFile(daemonPath));

            daemonCheckTimer = new Timer(Constants.DAEMON_RESTART_THREAD_INTERVAL);
            daemonCheckTimer.Elapsed += delegate(object source, ElapsedEventArgs e)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate(object sender, DoWorkEventArgs dwe)
                {
                    StartDaemonProcessMonitor(daemonPath, daemonArgs);
                };

                worker.RunWorkerAsync();
            };

            daemonCheckTimer.Start();
        }

        private void StartDaemonProcessMonitor(string exe, string args)
        {
            try
            {
                #region Check if the process we have already bound to is still running

                if (daemonPid != -1)
                {
                    try
                    {
                        Process dp = Process.GetProcessById(daemonPid);
                        if (dp == null || dp.HasExited)
                        {
                            Log.Instance.Write(Log_Severity.Warning, "CLI tool {0} exited unexpectedly. Restarting", exe);
                            daemonPid = -1;
                        }
                        else
                            return;
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.WriteNonFatalException(ex);
                        daemonPid = -1;
                    }
                }
                
                #endregion

                Process[] processes = Process.GetProcessesByName(Path.GetFileName(exe));

                #region Manage existing nervad processes

                if (processes.Length > 0)
                {
                    //Reconnect to an existing process if only one is running
                    //If more than 1, we have to start again, because we cannot be sure which one to connec to
                    if (processes.Length == 1 && Configuration.Instance.Daemon.ReconnectToDaemonProcess)
                    {
                        Process p = processes[0];
                        daemonPid = p.Id;
                        DaemonStarted?.Invoke(exe, args, p);
                    }
                    else
                    {
                        #region Kill existing instances

                        foreach (Process p in processes)
                            if (p.Id != daemonPid)
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

                        daemonPid = -1;
                                
                        #endregion
                    }
                }

                #endregion

                if (daemonPid == -1)
                {
                    #region Start a new daemon process

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
                    DaemonStarted?.Invoke(exe, args, proc);

                    proc.WaitForExit();

                    //We can assume there is only one nervad process running by now.
                    daemonPid = Process.GetProcessesByName(Path.GetFileName(exe))[0].Id;

                    #endregion
                }
                else
                    Log.Instance.Write("Connecting to process {0} with id {1}", exe, daemonPid);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }
        }

        public void StopDaemonCheck()
        {
            if (daemonCheckTimer != null)
                daemonCheckTimer.Stop();
        }
    }
}