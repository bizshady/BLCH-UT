using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AngryWasp.Logger;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit
{
    public partial class MainForm : Form
	{	
		Thread updateThread;
		bool shouldUpdate = true;

		public MainForm()
		{
			SuspendLayout();
			ConstructLayout();
			ResumeLayout();

			Application.Instance.Initialized += (s, e) =>
			{
				StartDaemonUpdateThread();
			};
		}

		private void StartDaemonUpdateThread()
		{
			updateThread = new Thread(new ThreadStart(UpdateUI));
			shouldUpdate = true;
			updateThread.Start();
		}

		private void StopDaemonUpdateThread()
		{
			shouldUpdate = false;
		}

		private void UpdateUI()
		{
			while (shouldUpdate)
			{
				Thread.Sleep(Constants.DAEMON_POLL_INTERVAL);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdate)
					break;

				Info info = null;
				List<Connection> connections = null;
				int height = -1;

				try
				{
					info = Cli.Instance.Daemon.GetInfo();
					connections = Cli.Instance.Daemon.GetConnections();
					height = Cli.Instance.Daemon.GetBlockCount();
				}
				catch (Exception)
				{
					//Log message will have already been written. No need to write another one here
				}

				//Double check we want to update before we do
				if (!shouldUpdate)
					break;

				if (info != null && connections != null && height != -1)
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						lblStatus.Text = $"Height: {height} | Connections (In/Out): {info.IncomingConnectionsCount} / {info.OutgoingConnectionsCount}";
						lblVersion.Text = $"Version: {info.Version}";
						ad.Version = $"GUI: {Constants.VERSION}\r\nCLI: {info.Version}";

						if (info.TargetHeight != 0 && info.Height < info.TargetHeight)
							lblStatus.Text += " | Syncing";
						else
							lblStatus.Text += " | Sync OK";
					});
				}
				else
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						lblStatus.Text = "ERROR: Could not connect to daemon";
					});

					Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);
				}
			}
		}

		protected void daemon_GetInfo_Clicked(object sender, EventArgs e)
		{
			MessageBox.Show("I was clicked!");
		}

		protected void daemon_GetConnections_Clicked(object sender, EventArgs e)
		{
			MessageBox.Show("I was clicked!");
		}

		protected void daemon_Restart_Clicked(object sender, EventArgs e)
		{
			//Log the restart and kill the daemon
			Log.Instance.Write("Restarting daemon");
			Cli.Instance.Daemon.StopDaemon();
			//From here the crash handler should reboot the daemon
		}

		protected void HandleAbout(object sender, EventArgs e)
		{
			ad.ShowDialog(this);
		}

		protected void HandleQuit(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
    }
}