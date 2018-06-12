using System;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Frontend
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			CommandLineParser cmd = CommandLineParser.Parse(args);

			string cmdPath = cmd["config-file"] != null ? cmd["config-file"].Value : null;
			string logPath = cmd["log-file"] != null ? cmd["log-file"].Value : null;

			Log.CreateInstance(true, logPath);
			Log.Instance.Write("NERVA Unified Toolkit. Version {0}", Constants.VERSION);

			if (logPath != null)
				Log.Instance.Write("Writing log to file '{0}'", logPath);

			Serializer.Initialize();
			Configuration.Load(cmdPath);
			CliInterface.Start();
			
			new Application(Eto.Platform.Detect).Run(new MainForm());

			Configuration.Save();
			Log.Instance.Shutdown();
		}
	}
}
