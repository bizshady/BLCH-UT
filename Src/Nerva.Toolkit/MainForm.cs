using System;
using System.Collections.Generic;
using System.Threading;
using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit
{
    public partial class MainForm : Form
	{	
		Thread updateDaemonThread;
		bool shouldUpdateDaemon = true;

		Thread updateWalletThread;
		bool shouldUpdateWallet = true;

		Thread pingThread;

		bool pingSuccess = true;

		public MainForm()
		{
			SuspendLayout();
			ConstructLayout();
			ResumeLayout();

			Cli.Instance.Start();

			Application.Instance.Initialized += (s, e) =>
			{
				StartUpdateThread();
			};

			this.Closing += (s, e) =>
			{
				shouldUpdateDaemon = false;
				shouldUpdateWallet = false;
			};
		}

		private void StartUpdateThread()
		{
			updateDaemonThread = new Thread(new ThreadStart(UpdateDaemonUI));
			shouldUpdateDaemon = true;
			updateDaemonThread.Start();

			updateWalletThread = new Thread(new ThreadStart(UpdateWalletUI));
			shouldUpdateWallet = true;
			updateWalletThread.Start();

			pingThread = new Thread(new ThreadStart(CheckConnection));
			pingSuccess = true;
			pingThread.Start();
		}

		private void UpdateDaemonUI()
		{
			while (shouldUpdateDaemon)
			{
				Thread.Sleep(Constants.DAEMON_POLL_INTERVAL);

				//spin the wheels for a bit if we should be updating, but have no daemon
				while(shouldUpdateDaemon && Cli.Instance.DaemonPid == -1)
					Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);

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
						lblStatus.Text = "NOT CONNECTED TO DAEMON";
						daemonPage.Update(null, null, null);
					});
					
					//Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);
				}
			}
		}

		private void UpdateWalletUI()
		{
			while (shouldUpdateWallet)
			{
				Thread.Sleep(Constants.DAEMON_POLL_INTERVAL);

				//spin the wheels for a bit if we should be updating, but have no wallet
				while(shouldUpdateWallet && Cli.Instance.WalletPid == -1)
					Thread.Sleep(Constants.DAEMON_RESTART_THREAD_INTERVAL);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdateWallet)
					break;

				Account account = null;

				try
				{
					account = Cli.Instance.Wallet.GetAccounts();
				}
				catch (Exception) { }

				//Double check we want to update before we do
				if (!shouldUpdateWallet)
					break;

				if (account != null)
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						walletPage.Update(account);
					});
				}
				else
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						walletPage.Update(null);
					});
				}
			}
		}

		private void CheckConnection()
		{
			while (true)
			{
				//If last ping failed. speed up this wait time. Do the online check as quick as reasonably possible
				Thread.Sleep(pingSuccess ? Constants.DAEMON_POLL_INTERVAL * 5 : Constants.DAEMON_POLL_INTERVAL);

				//Try pinging all 3 seed nodes before calling it disconnected
				bool pingOk = NetHelper.PingServer(SeedNodes.XNV1);

				if (!pingOk)
					pingOk = NetHelper.PingServer(SeedNodes.XNV2);

				if (!pingOk)
					pingOk = NetHelper.PingServer(SeedNodes.XNV3);

				pingSuccess = pingOk;

				if (!pingOk)
				{
					shouldUpdateDaemon = false;
					shouldUpdateWallet = false;

					Application.Instance.AsyncInvoke(() =>
					{
						lblStatus.Text = "NOT CONNECTED TO INTERNET";
						daemonPage.Update(null, null, null);
					});

					continue;
				}
				else
				{
					shouldUpdateDaemon = true;
					shouldUpdateWallet = true;
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