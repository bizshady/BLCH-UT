using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        bool updateDaemon = true;
        bool updateWallet = true;

        System.Timers.Timer updateWalletTimer = new System.Timers.Timer(Constants.ONE_SECOND);
        System.Timers.Timer updateDaemonTimer = new System.Timers.Timer(Constants.ONE_SECOND);

        bool pingSuccess = true;
        uint lastTxHeight = 0;

        public MainForm(bool newConfig)
        {
            bool needSetup = newConfig || !FileNames.DirectoryContainsCliTools(Configuration.Instance.ToolsPath);
            do
            {
                if (needSetup)
                {
                    DialogResult dr = new PreferencesDialog().ShowModal();
                    needSetup = !FileNames.DirectoryContainsCliTools(Configuration.Instance.ToolsPath);

                    if (needSetup)
                        MessageBox.Show(this, "Could not find the NERVA CLI tools at the specified path.", "Invalid Config",
                            MessageBoxButtons.OK, MessageBoxType.Warning, MessageBoxDefaultButton.OK);

                    //if cancelled. close the program. otherwise loop back and try again		
                    if (dr == DialogResult.Cancel)
                        Environment.Exit(0);
                }
            }
            while (needSetup);

            Configuration.Save();

            SuspendLayout();
            ConstructLayout();
            ResumeLayout();

            Cli.Instance.StartDaemon();
            Cli.Instance.StartWallet();

            Application.Instance.Initialized += (s, e) =>
            {
                CreateUpdateDaemonTimer();
                CreateUpdateWalletTimer();

                StartUpdateDaemonTimer();
                StartUpdateWalletTimer();
            };

            this.Closing += (s, e) =>
            {
                StopUpdateDaemonTimer();
                StopUpdateWalletTimer();
                Cli.Instance.Wallet.ForceClose();
            };
        }

        public void CreateUpdateDaemonTimer()
        {
            updateDaemonTimer.Elapsed += (s, e) =>
            {
                if (!updateDaemon)
                {
                    updateDaemonTimer.Stop();
                    return;
                }

                if (Cli.Instance.Daemon.Pid == -1)
                    return;

                updateDaemonTimer.Stop();

                Task.Run(() =>
            	{
                	UpdateDaemonUI();
            		updateDaemonTimer.Start();
            	});
            };
        }

        public void CreateUpdateWalletTimer()
        {
            updateWalletTimer.Elapsed += (s, e) =>
            {
                if (!updateWallet)
                {
                    updateWalletTimer.Stop();
                    return;
                }

                if (Cli.Instance.Wallet.Pid == -1)
                    return;

                updateWalletTimer.Stop();

                Task.Run(() =>
            	{
                	UpdateWalletUI();
                	updateWalletTimer.Start();
            	});
            };
        }

        public void StartUpdateDaemonTimer()
        {
            if (updateDaemonTimer.Enabled)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }

            updateDaemon = true;
            updateDaemonTimer.Start();
        }

        public void StopUpdateDaemonTimer()
        {
            updateDaemon = false;
            updateDaemonTimer.Stop();
        }

        public void StartUpdateWalletTimer()
        {
            if (updateWalletTimer.Enabled)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }

            updateWallet = true;
            updateWalletTimer.Start();
        }

        public void StopUpdateWalletTimer()
        {
            updateWallet = false;
            updateWalletTimer.Stop();
        }

        private void UpdateDaemonUI()
        {
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

            if (info != null)
            {
                Application.Instance.Invoke(() =>
            	{
                	lblDaemonStatus.Text = $"Height: {height} | Connections: {info.IncomingConnectionsCount}/{info.OutgoingConnectionsCount}";

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
            	Application.Instance.Invoke(() =>
            	{
                	lblDaemonStatus.Text = "OFFLINE";
                	daemonPage.Update(null, null, null);
            	});
            }
        }

        private void UpdateWalletUI()
        {
            Account account = null;
            TransferList transfers = null;

            try
            {
                account = Cli.Instance.Wallet.Interface.GetAccounts();
                transfers = Cli.Instance.Wallet.Interface.GetTransfers(lastTxHeight, out lastTxHeight);
            }
            catch (Exception) { }

            if (account != null)
            {
                string walletStatus = (account != null) ? $"Account(s): {account.Accounts.Count}  | Balance: {Conversions.FromAtomicUnits(account.TotalBalance)} XNV" : "WALLET CLOSED";

                Application.Instance.Invoke(() =>
              	{
                	lblWalletStatus.Text = walletStatus;
                	balancesPage.Update(account);
                	transfersPage.Update(transfers);
              	});
            }
        }

        private void CheckConnection()
        {
            /*while (true)
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

					Application.Instance.Invoke(() =>
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
			}*/
        }

        private void CloseWallet(bool clearSavedWallet)
        {
            Log.Instance.Write("Closing wallet");
            Cli.Instance.Wallet.StopCrashCheck();
            StopUpdateWalletTimer();
            lastTxHeight = 0;

            balancesPage.Update(null);
            transfersPage.Update(null);
            lblWalletStatus.Text = "OFFLINE | CLOSED";

            Cli.Instance.Wallet.ForceClose();

            if (clearSavedWallet)
            {
                Configuration.Instance.Wallet.LastOpenedWallet = null;
                Configuration.Instance.Wallet.LastWalletPassword = null;
                Configuration.Save();
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
                CloseWallet(false);
                Cli.Instance.Wallet.ResumeCrashCheck();
                StartUpdateWalletTimer();
            }
        }

        protected void wallet_Store_Clicked(object sender, EventArgs e)
        {
			Task.Run(() =>
           	{
               	Log.Instance.Write("Saving wallet");
            	Cli.Instance.Wallet.Interface.Store();

               	Application.Instance.AsyncInvoke(() =>
               	{
                   	MessageBox.Show(this, "Wallet Save Complete", "NERVA Wallet", MessageBoxButtons.OK,
                		MessageBoxType.Information, MessageBoxDefaultButton.OK);
               	});
           	});
        }

        protected void wallet_Stop_Clicked(object sender, EventArgs e)
        {
            CloseWallet(true);
        }

        protected void wallet_RescanSpent_Clicked(object sender, EventArgs e)
        {
            Task.Run(() =>
           	{
               	Log.Instance.Write("Rescanning spent outputs");
               	if (!Cli.Instance.Wallet.Interface.RescanSpent())
                   	Log.Instance.Write("Rescanning spent outputs failed");
               	else
                   	Log.Instance.Write("Rescanning spent outputs success");

               	Application.Instance.AsyncInvoke(() =>
               	{
                   	MessageBox.Show(this, "Rescanning spent outputs complete", "Rescan Spent",
                    	MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
               	});
           	});
        }

        protected void wallet_RescanBlockchain_Clicked(object sender, EventArgs e)
        {
            Task.Run(() =>
        	{
               	Log.Instance.Write("Rescanning blockchain");
               	if (!Cli.Instance.Wallet.Interface.RescanBlockchain())
            		Log.Instance.Write("Rescanning blockchain failed");
            	else
                	Log.Instance.Write("Rescanning blockchain success");

            	Application.Instance.AsyncInvoke(() =>
            	{
                	MessageBox.Show(this, "Rescanning blockchain complete", "Rescan Blockchain",
                    	MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
               	});
        	});
        }

        protected void wallet_Keys_View_Clicked(object sender, EventArgs e)
        {
            new DisplayKeysDialog().ShowModal();
        }

        protected void wallet_Account_Create_Clicked(object sender, EventArgs e)
        {
            TextDialog d = new TextDialog("Enter Account Name", false);
            if (d.ShowModal() == DialogResult.Ok)
            {
                Task.Run(() =>
            	{
                	if (Cli.Instance.Wallet.Interface.CreateAccount(d.Text) == null)
                	{
                    	Application.Instance.AsyncInvoke(() =>
                    	{
                        	MessageBox.Show(this, "Failed to create new account", "Create Account",
                        		MessageBoxButtons.OK, MessageBoxType.Error, MessageBoxDefaultButton.OK);
                    	});
                	}
            	});
            }
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
                    Cli.Instance.Daemon.ForceClose();
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