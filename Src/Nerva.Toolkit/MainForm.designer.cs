using Eto.Forms;
using Eto.Drawing;
using System.Reflection;
using Nerva.Toolkit.Content;

namespace Nerva.Toolkit
{
    public partial class MainForm : Form
	{	
		#region Status Bar controls
		
		AboutDialog ad;
		Label lblStatus = new Label { Text = "Connections  (In/Out): 0 / 0" };
		Label lblVersion = new Label { Text = "Version: 0.0.0.0" };

		DaemonPage daemonPage = new DaemonPage();
		BalancesPage balancesPage = new BalancesPage();
		TransfersPage transfersPage = new TransfersPage();

		#endregion

		public void ConstructLayout()
		{
			Title = "NERVA Toolkit";
			ClientSize = new Size(640, 480);
			
			//Construct About dialog
			ad = new AboutDialog();
			ad.ProgramName = "NERVA Unified Toolkit";
			ad.ProgramDescription = "Unified frontend for the NERVA CLI tools";
			string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			ad.Title = "About NERVA Toolkit";
			ad.License = "Copyright © 2018 Angry Wasp";
			ad.Logo = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("Nerva.Toolkit.NERVA-Logo.png"));

			daemonPage.ConstructLayout();
			balancesPage.ConstructLayout();
			transfersPage.ConstructLayout();

			TabControl tabs = new TabControl
			{
				Pages = {
					new TabPage { Text = "Daemon", Content = daemonPage.MainControl },
					new TabPage { Text = "Balances", Content = balancesPage.MainControl },
					new TabPage { Text = "Transfers", Content = transfersPage.MainControl },
				}
			};

			TableLayout statusBar = new TableLayout
			{
				Padding = 5,
				Rows = {
					new TableRow (
						new TableCell(lblStatus, true),
						new TableCell(lblVersion))
				}
			};

			Content = new TableLayout
			{
				Rows = {
					new TableRow (
						new TableCell(tabs, true)) { ScaleHeight = true },
					new TableRow (
						new TableCell(statusBar, true))
				}
			};

			var file_Preferences = new Command { MenuText = "Preferences", ToolBarText = "Preferences" };	

			var daemon_ToggleMining = new Command { MenuText = "Toggle Miner", ToolBarText = "Toggle Miner" };			
			daemon_ToggleMining.Executed += daemon_ToggleMining_Clicked;

			var daemon_Restart = new Command { MenuText = "Restart", ToolBarText = "Restart" };
			daemon_Restart.Executed += daemon_Restart_Clicked;

			var wallet_Select = new Command { MenuText = "Select", ToolBarText = "Select" };
			wallet_Select.Executed += wallet_Select_Clicked;

			var wallet_Store = new Command { MenuText = "Save", ToolBarText = "Save" };
			wallet_Store.Executed += wallet_Store_Clicked;

			var wallet_RescanSpent = new Command { MenuText = "Spent Outputs", ToolBarText = "Spent Outputs" };
			wallet_RescanSpent.Executed += wallet_RescanSpent_Clicked;

			var wallet_RescanBlockchain = new Command { MenuText = "Spent Blockchain", ToolBarText = "Spent Blockchain" };
			wallet_RescanBlockchain.Executed += wallet_RescanBlockchain_Clicked;

			var wallet_Keys_View = new Command { MenuText = "View Keys", ToolBarText = "View Keys" };
			wallet_Keys_View.Executed += wallet_Keys_View_Clicked;

			var wallet_Account_Create = new Command { MenuText = "Create Account", ToolBarText = "Create Account" };
			wallet_Account_Create.Executed += wallet_Account_Create_Clicked;

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += quit_Clicked;

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += about_Clicked;

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem
					{ 
						Text = "&File",
						Items =
						{ 
							file_Preferences
						}
					},
					new ButtonMenuItem
					{
						Text = "&Daemon",
						Items =
						{
							daemon_ToggleMining,
							daemon_Restart
						}
					},
					new ButtonMenuItem
					{
						Text = "&Wallet",
						Items =
						{
							wallet_Select,
							new SeparatorMenuItem(),
							wallet_Store,
							wallet_Account_Create,
							new ButtonMenuItem
							{
								Text = "Rescan",
								Items =
								{
									wallet_RescanSpent,
									wallet_RescanBlockchain
								}
							},
							wallet_Keys_View
						}
					}
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
		}
	}
}
