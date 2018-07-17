using System;
using System.IO;
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
			var platform = Eto.Platform.Detect;

			CommandLineParser cmd = CommandLineParser.Parse(args);

			string logFile, configFile;

			ParseFileArguments(cmd, out logFile, out configFile);

			InitializeLog(logFile);

			Serializer.Initialize();

			bool newFile;

			Configuration.Load(configFile, out newFile);
			Cli.Instance.KillCliProcesses(FileNames.RPC_WALLET);

			Configuration.Instance.NewDaemonOnStartup = cmd["new-daemon"] != null;

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
				new Application(platform).Run(new MainForm(newFile));
			}
			catch (Exception ex)
			{
				Log.Instance.WriteNonFatalException(ex);

				Cli.Instance.Daemon.StopCrashCheck();
				Cli.Instance.Wallet.StopCrashCheck();

				Cli.Instance.Wallet.ForceClose();
				Cli.Instance.Daemon.ForceClose();

				Configuration.Save();
				Log.Instance.Write(Log_Severity.Fatal, "PROGRAM TERMINATED");
			}

			//Prevent the daemon restarting automatically before telling it to stop
			if (Configuration.Instance.Daemon.StopOnExit)
			{
				Cli.Instance.Daemon.StopCrashCheck();
				Cli.Instance.Daemon.Interface.StopDaemon();
			}

			//be agressive and make sure it is dead
			Cli.Instance.Wallet.StopCrashCheck();
			Cli.Instance.Wallet.ForceClose();

			Configuration.Save();
			Log.Instance.Shutdown();

			Environment.Exit(0);
		}

		private static void ParseFileArguments(CommandLineParser cmd, out string logFile, out string configFile)
		{
			logFile = Path.Combine(Environment.CurrentDirectory, Constants.DEFAULT_LOG_FILENAME);
			configFile = Path.Combine(Environment.CurrentDirectory, Constants.DEFAULT_CONFIG_FILENAME);

			var lf = cmd["log-file"];
			var cf = cmd["config-file"];

			if (lf != null && !string.IsNullOrEmpty(lf.Value))
			{
				string newLf = Path.GetFullPath(lf.Value);
				if (Directory.Exists(Path.GetDirectoryName(newLf)))
					logFile = newLf;
			}

			if (cf != null && !string.IsNullOrEmpty(cf.Value))
			{
				string newCf = Path.GetFullPath(cf.Value);
				if (Directory.Exists(Path.GetDirectoryName(newCf)))
					configFile = newCf;
			}
		}	

		private static void InitializeLog(string logPath)
		{
			Log.CreateInstance(true, logPath);
			Log.Instance.Write("NERVA Unified Toolkit. Version {0}", Constants.LONG_VERSION);

			//Crash the program if not 64-bit
			if (!Environment.Is64BitOperatingSystem)
				Log.Instance.Write(Log_Severity.Fatal, "The NERVA Unified Toolkit is only available for 64-bit platforms");

			Log.Instance.Write(Log_Severity.None, "System Information:");
			Log.Instance.Write(Log_Severity.None, "OS: {0} {1}", Environment.OSVersion.Platform, Environment.OSVersion.Version);
			Log.Instance.Write(Log_Severity.None, "CPU Count: {0}", Environment.ProcessorCount);
			
			if (logPath != null)
				Log.Instance.Write("Writing log to file '{0}'", logPath);
		}
	}
}
