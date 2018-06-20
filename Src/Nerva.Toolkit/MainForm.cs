using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Content.Dialogs;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit
{
    public partial class MainForm : Form
	{	
		Thread updateDaemonThread;
		bool shouldUpdateDaemon = true;

		Thread updateWalletThread;
		bool shouldUpdateWallet = true;

		public MainForm()
		{
			SuspendLayout();
			ConstructLayout();
			ResumeLayout();

			var w = Configuration.Instance.Wallet;

			if (w.LastOpenedWallet != null)
			{
				string walletFile = Path.Combine(w.WalletDir, w.LastOpenedWallet);

				if (File.Exists(walletFile))
				{
					//Wallet file is saved in config and exists on disk.
					//Load from the saved password if that exists
					string password = null;
					if (w.LastWalletPassword != null)
						password = w.LastOpenedWallet.DecodeBase64();

					while (true)
					{
						if (password == null)
						{
							EnterPasswordDialog d = new EnterPasswordDialog();
							if (d.ShowModal() == DialogResult.Ok)
								password = d.Password;
						}
					}
				}
			}

			Application.Instance.Initialized += (s, e) =>
			{
				StartUpdateThread();
			};

			/**/
		}

		private void StartUpdateThread()
		{
			updateDaemonThread = new Thread(new ThreadStart(UpdateDaemonUI));
			shouldUpdateDaemon = true;
			updateDaemonThread.Start();

			updateWalletThread = new Thread(new ThreadStart(UpdateWalletUI));
			shouldUpdateWallet = true;
			updateWalletThread.Start();
		}

		private void StopDaemonUpdateThread()
		{
			shouldUpdateDaemon = false;
		}

		private void UpdateDaemonUI()
		{
			while (shouldUpdateDaemon)
			{
				Thread.Sleep(Constants.DAEMON_POLL_INTERVAL);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdateDaemon)
					break;

				Info info = null;
				List<Connection> connections = null;
				int height = -1;
				MiningStatus mStatus = null;

				try
				{
					info = Cli.Instance.Daemon.GetInfo();
					connections = Cli.Instance.Daemon.GetConnections();
					height = Cli.Instance.Daemon.GetBlockCount();
					mStatus = Cli.Instance.Daemon.GetMiningStatus();
				}
				catch (Exception) { }

				//Double check we want to update before we do
				if (!shouldUpdateDaemon)
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

						daemonPage.Update(info, connections, mStatus);
					});
				}
				else
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						lblStatus.Text = "ERROR: Could not connect to daemon";
						daemonPage.Update(null, null, null);
					});

					Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);
				}
			}
		}

		private void UpdateWalletUI()
		{
			while (shouldUpdateWallet)
			{
				Thread.Sleep(Constants.DAEMON_POLL_INTERVAL);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdateWallet)
					break;

				//TODO: Update wallet releated GUI elements
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