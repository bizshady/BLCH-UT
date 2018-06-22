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
			Configuration.Load(cmdPath);
			Cli.CreateInstance();

			Configuration.Instance.NewDaemonOnStartup = cmd["new-daemon"] != null;
			Configuration.Instance.NewWalletOnStartup = cmd["new-wallet"] != null;

			new Application(Eto.Platform.Detect).Run(new MainForm());

			//Prevent the daemon restarting automatically before telling it to stop
			if (Configuration.Instance.Daemon.StopOnExit)
			{
				Cli.Instance.StopDaemonCheck();
				Cli.Instance.Daemon.StopDaemon();
			}

			if (Configuration.Instance.Wallet.StopOnExit)
			{
				Cli.Instance.StopDaemonCheck();
				Cli.Instance.Daemon.StopDaemon();
			}

			Configuration.Save();
			Log.Instance.Shutdown();
		}
	}
}
