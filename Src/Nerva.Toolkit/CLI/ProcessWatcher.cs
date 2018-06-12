using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using AngryWasp.Logger;
using System.Threading;

namespace Nerva.Toolkit.CLI
{
    /// <summary>
    /// Watches the system processes to make sure a process is running.
    /// This class only allows a single instance of a process to run.
    /// </summary>
    public class ProcessWatcher
    {
        public void StartWatcherThread(string processName, string args)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                StartProcess(processName, args);
            };

            worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                Log.Instance.Write(Log_Severity.Warning, "CLI tool {0} exited unexpectedly. Restarting", processName);
                worker.RunWorkerAsync();
            };

            worker.RunWorkerAsync();
        }

        public void StartProcess(string exe, string args)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(Path.GetFileName(exe));
                Process proc;

                //Kill existing nervad instances. We need to start a new one with our config file parameters
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

                ProcessStartInfo psi = new ProcessStartInfo(exe, args);
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;

                proc = new Process();
                proc.StartInfo = psi;
                proc.EnableRaisingEvents = true;

                proc.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    Console.WriteLine(e.Data);
                };

                proc.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    Console.WriteLine(e.Data);
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }
        }
    }
}