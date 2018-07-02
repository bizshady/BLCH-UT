using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Frontend
{
	class MainClass
	{
		/// <summary>
		/// Program entry point
		/// Available command line arguments
		/// --log-file: Location to write a log file to
		/// --config-file: Location to load a config file from
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			CommandLineParser cmd = CommandLineParser.Parse(args);

			string cmdPath = cmd["config-file"] != null ? cmd["config-file"].Value : Constants.DEFAULT_CONFIG_FILENAME;
			string logPath = cmd["log-file"] != null ? cmd["log-file"].Value : Constants.DEFAULT_LOG_FILENAME;
			
			Log.CreateInstance(true, logPath);
			Log.Instance.Write("NERVA Unified Toolkit. Version {0}", Constants.VERSION);

			//Crash the program if not 64-bit
			if (!Environment.Is64BitOperatingSystem)
				Log.Instance.Write(Log_Severity.Fatal, "The NERVA Unified Toolkit is only available for 64-bit platforms");

			Log.Instance.Write(Log_Severity.None, "System Information:");
			Log.Instance.Write(Log_Severity.None, "OS: {0} {1}", Environment.OSVersion.Platform, Environment.OSVersion.Version);
			Log.Instance.Write(Log_Severity.None, "CPU Count: {0}", Environment.ProcessorCount);
			
			if (logPath != null)
				Log.Instance.Write("Writing log to file '{0}'", logPath);

			Serializer.Initialize();

			bool newFile;

			Configuration.Load(cmdPath, out newFile);

			if (newFile)
				Environment.Exit(0);

			Cli.CreateInstance();

			Cli.Instance.KillRunningProcesses(FileNames.RPC_WALLET);

			Configuration.Instance.NewDaemonOnStartup = cmd["new-daemon"] != null;

			var platform = Eto.Platform.Detect;

			if (platform.IsGtk)
			{
				GLib.ExceptionManager.UnhandledException += (x) =>
				{
					var ex = x.ExceptionObject as Exception;
					Log.Instance.WriteFatalException(ex);
				};
			}

			try
			{
				new Application(platform).Run(new MainForm());
			}
			catch (Exception ex)
			{
				Log.Instance.WriteNonFatalException(ex);
				Cli.Instance.StopDaemonCheck();
				Cli.Instance.StopWalletCheck();
				//Error. Force close all CLI tools
				Cli.Instance.KillRunningProcesses(FileNames.CLI_WALLET);
				Cli.Instance.KillRunningProcesses(FileNames.RPC_WALLET);
				Cli.Instance.KillRunningProcesses(FileNames.NERVAD);

				Configuration.Save();
				Log.Instance.Write(Log_Severity.Fatal, "PROGRAM TERMINATED");
			}

			//Prevent the daemon restarting automatically before telling it to stop
			if (Configuration.Instance.Daemon.StopOnExit)
			{
				Cli.Instance.StopDaemonCheck();
				Cli.Instance.Daemon.StopDaemon();
			}

			//be agressive and make sure it is dead
			Cli.Instance.StopWalletCheck();
			Cli.Instance.KillRunningProcesses(FileNames.RPC_WALLET);

			Configuration.Save();
			Log.Instance.Shutdown();
		}
	}
}
