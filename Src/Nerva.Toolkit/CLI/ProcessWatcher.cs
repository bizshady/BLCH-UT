using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Nerva.Toolkit.CLI
{
    /// <summary>
    /// Watches the system processes to make sure a process is running.
    /// This class only allows a single instance of a process to run.
    /// </summary>
    public class ProcessWatcher
    {
        private Dictionary<string, Process> processes = new Dictionary<string, Process>();

        public void StartWatcherThread(string processName, string args)
        {
            string executable = Path.GetFileName(processName);
            Process process = StartProcess(executable);
            processes.Add(executable, process);

            //TODO: start monitor thread
        }

        public Process StartProcess(string executable)
        {
            return null;
        }
    }
}