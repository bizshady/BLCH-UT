using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

		Thread pingThread;

		bool pingSuccess = true;
		uint lastTxHeight = 0;

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
				Cli.Instance.Wallet.ForceClose();
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

			//pingThread = new Thread(new ThreadStart(CheckConnection));
			//pingSuccess = true;
			//pingThread.Start();
		}

		private void UpdateDaemonUI()
		{
			while (shouldUpdateDaemon)
			{
				Thread.Sleep(Constants.ONE_SECOND);

				//spin the wheels for a bit if we should be updating, but have no daemon
				while(shouldUpdateDaemon && Cli.Instance.Daemon.Pid == -1)
					Thread.Sleep(Constants.ONE_SECOND);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdateDaemon)
					break;

				Info info = null;
				List<Connection> connections = null;
				int height = -1;
				MiningStatus mStatus = null;

				try
				{
					height = Cli.Instance.Daemon.Interface.GetBlockCount();
					info = Cli.Instance.Daemon.Interface.GetInfo();
					connections = Cli.Instance.Daemon.Interface.GetConnections();
					mStatus = Cli.Instance.Daemon.Interface.GetMiningStatus();
				}
				catch (Exception) { }

				//Double check we want to update before we do
				if (!shouldUpdateDaemon)
					break;

				if (info != null && connections != null && height != -1)
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						lblDaemonStatus.Text = $"DAEMON > Height: {height} | Connections: {info.IncomingConnectionsCount}/{info.OutgoingConnectionsCount}";

						if (info.TargetHeight != 0 && info.Height < info.TargetHeight)
							lblDaemonStatus.Text += " | Syncing";
						else
							lblDaemonStatus.Text += " | Sync OK";

						lblVersion.Text = $"Version: {info.Version}";
						ad.Version = $"GUI: {Constants.VERSION}\r\nCLI: {info.Version}";

						daemonPage.Update(info, connections, mStatus);
					});
				}
				else
				{
					Application.Instance.AsyncInvoke ( () =>
					{
						lblDaemonStatus.Text = "DAEMON > OFFLINE";
						daemonPage.Update(null, null, null);
					});
				}
			}
		}

		private void UpdateWalletUI()
		{
			while (shouldUpdateWallet)
			{
				Thread.Sleep(Constants.ONE_SECOND);

				//spin the wheels for a bit if we should be updating, but have no wallet
				while(shouldUpdateWallet && Cli.Instance.Wallet.Pid == -1)
					Thread.Sleep(Constants.ONE_SECOND);

				//Condition may have changed. 5 seconds is a long time
				if (!shouldUpdateWallet)
					break;

				Account account = null;
				TransferList transfers = null;

				try
				{
					account = Cli.Instance.Wallet.Interface.GetAccounts();
					transfers = Cli.Instance.Wallet.Interface.GetTransfers(lastTxHeight, out lastTxHeight);
				}
				catch (Exception ex)
				{
					Log.Instance.WriteNonFatalException(ex);
				}

				//Double check we want to update before we do
				if (!shouldUpdateWallet)
					break;
				
				string wOffline = ((Cli.Instance.Wallet.Pid == -1)) ? "OFFLINE" : "ONLINE";
				string wOpen = (account != null) ? $"Account(s): {account.Accounts.Count}  | Balance: {Conversions.FromAtomicUnits(account.TotalBalance)} XNV" : "CLOSED";

				Application.Instance.AsyncInvoke ( () =>
				{
					lblWalletStatus.Text = $"WALLET > {wOffline} | {wOpen}";

					balancesPage.Update(account);
					transfersPage.Update(transfers);
				});
			}
		}

		private void CheckConnection()
		{
			while (true)
			{
				//If last ping failed. speed up this wait time. Do the online check as quick as reasonably possible
				Thread.Sleep(pingSuccess ? Constants.FIVE_SECONDS : Constants.ONE_SECOND);

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
						lblDaemonStatus.Text = "NOT CONNECTED TO INTERNET";
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

		protected void daemon_ToggleMining_Clicked(object sender, EventArgs e)
		{
			MiningStatus ms = Cli.Instance.Daemon.Interface.GetMiningStatus();

			if (ms.Active)
			{
				Cli.Instance.Daemon.Interface.StopMining();
				Log.Instance.Write("Mining stopped"); 
			}
			else
				if (Cli.Instance.Daemon.Interface.StartMining())
					Log.Instance.Write("Mining started for @ {0} on {1} threads", 
						Conversions.WalletAddressShortForm(Configuration.Instance.Daemon.MiningAddress),
						Configuration.Instance.Daemon.MiningThreads);
		}

		protected void daemon_Restart_Clicked(object sender, EventArgs e)
		{
			//Log the restart and kill the daemon
			Log.Instance.Write("Restarting daemon");
			Cli.Instance.Daemon.Interface.StopDaemon();
			//From here the crash handler should reboot the daemon
		}

		protected void wallet_Select_Clicked(object sender, EventArgs e)
		{
			if (WalletHelper.ShowWalletWizard())
			{
				//HACK: RPC wallet sometimes hangs after opening a new wallet. So the wizard will 
				//save the wallet info and we Kill the process and let it relaunch
				//We have to physically kill the process as calling stop will not work if it is unresponsive
				lastTxHeight = 0;
				Cli.Instance.Wallet.ForceClose();
				transfersPage.Update(null);
			}
		}

		protected void wallet_Store_Clicked(object sender, EventArgs e)
		{
			Log.Instance.Write("Storing blockchain");
			Cli.Instance.Wallet.Interface.Store();
			MessageBox.Show(this, "Wallet Save Complete", "NERVA Wallet", MessageBoxButtons.OK, 
				MessageBoxType.Information, MessageBoxDefaultButton.OK);
		}

		protected void wallet_RescanSpent_Clicked(object sender, EventArgs e)
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (ws, we) =>
			{
				Log.Instance.Write("Rescanning spent outputs");
				if (!Cli.Instance.Wallet.Interface.RescanSpent())
					Log.Instance.Write("Rescanning spent outputs failed");
				else
					Log.Instance.Write("Rescanning spent outputs success");
			};

			w.RunWorkerCompleted += (ws, we) =>
			{
				MessageBox.Show(this, "Rescanning spent outputs complete", "Rescan Spent", 
					MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
			};

			w.RunWorkerAsync();
		}

		protected void wallet_RescanBlockchain_Clicked(object sender, EventArgs e)
		{
			BackgroundWorker w = new BackgroundWorker();

			w.DoWork += (ws, we) =>
			{
				Log.Instance.Write("Rescanning blockchain");
				if (!Cli.Instance.Wallet.Interface.RescanBlockchain())
					Log.Instance.Write("Rescanning blockchain failed");
				else
					Log.Instance.Write("Rescanning blockchain success");
			};

			w.RunWorkerCompleted += (ws, we) =>
			{
				MessageBox.Show(this, "Rescanning blockchain complete", "Rescan Blockchain", 
					MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
			};

			w.RunWorkerAsync();
		}

		protected void wallet_Keys_View_Clicked(object sender, EventArgs e)
		{
			new DisplayKeysDialog().ShowModal();
		}

		protected void wallet_Account_Create_Clicked(object sender, EventArgs e)
		{
			TextDialog d = new TextDialog("Enter Account Name", false);
			if (d.ShowModal() == DialogResult.Ok)
				if (Cli.Instance.Wallet.Interface.CreateAccount(d.Text) == null)
					MessageBox.Show(this, "Failed to create new account", "Create Account", MessageBoxButtons.OK, MessageBoxType.Error, MessageBoxDefaultButton.OK);
		}

		protected void about_Clicked(object sender, EventArgs e)
		{
			ad.ShowDialog(this);
		}

		protected void file_Preferences_Clicked(object sender, EventArgs e)
		{
			PreferencesDialog d = new PreferencesDialog();
			if (d.ShowModal() == DialogResult.Ok)
			{
				Configuration.Save();

				if (d.RestartDaemonRequired)
				{
					//if thge daemon has to be restarted, there is a good chance the wallet has to be restarted, so just do it
					MessageBox.Show(this, "The NERVA daemon will now restart to apply your changes", "NERVA Preferences", 
						MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);

					Log.Instance.Write("Restarting daemon");
					Cli.Instance.Daemon.Interface.StopDaemon();

					Cli.Instance.Wallet.ForceClose();
				}
				else
				{
					if (d.RestartMinerRequired)
					{
						Cli.Instance.Daemon.Interface.StopMining();
						Cli.Instance.Daemon.Interface.StartMining();
					}
				}

				if (d.RestartWalletRequired)
				{
					MessageBox.Show(this, "The NERVA RPC wallet will now restart to apply your changes", "NERVA Preferences", 
						MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);

					Cli.Instance.Wallet.ForceClose();
				}
			}	
		}

		protected void quit_Clicked(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
    }
}