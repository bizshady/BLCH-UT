using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;

namespace Nerva.Toolkit
{	
	public partial class MainForm : Form
	{	
		#region Status Bar controls

		Label lblStatus = new Label { Text = "Connections  (In/Out): 0 / 0" };
		Label lblVersion = new Label { Text = "Version: 0.1.2.3" };

		#endregion

		public void ConstructLayout()
		{
			Title = "NERVA Toolkit";
			ClientSize = new Size(640, 480);

			var statusPage = new TabPage { Text = "Status", Content = new StatusPage() };
			var minerPage = new TabPage { Text = "Miner", Content = new MinerPage() };
			var sendPage = new TabPage { Text = "Send", Content = new SendPage() };
			var transactionsPage = new TabPage { Text = "Transactions", Content = new TransactionsPage() };

			TabControl tabs = new TabControl
			{
				Pages = 
				{
					statusPage,
					minerPage,
					sendPage,
					transactionsPage
				}
			};

			TableLayout statusBar = new TableLayout
			{
				Padding = 5,
				Rows =
				{
					new TableRow
					{
						Cells = 
						{
							new TableCell(lblStatus, true),
							new TableCell(lblVersion),
						}
					}
				}
			};

			TableLayout content = new TableLayout
			{
				Rows =
				{
					new TableRow (new TableCell(tabs, true))
					{
						ScaleHeight = true
					},
					new TableRow (new TableCell(statusBar, true))
				}
			};

			Content = content;

			var daemon_GetInfo = new Command { MenuText = "Show Info", ToolBarText = "Show Info" };
			daemon_GetInfo.Executed += daemon_GetInfo_Clicked;

			var daemon_GetConnections = new Command { MenuText = "Show Connections", ToolBarText = "Show Connections" };
			daemon_GetConnections.Executed += daemon_GetConnections_Clicked;

			var daemon_Restart = new Command { MenuText = "Restart", ToolBarText = "Restart" };
			daemon_Restart.Executed += daemon_Restart_Clicked;

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += HandleQuit;

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += HandleAbout;

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
							
						}
					},
					new ButtonMenuItem
					{
						Text = "&Daemon",
						Items =
						{
							daemon_GetInfo,
							daemon_GetConnections,
							new SeparatorMenuItem(),
							daemon_Restart
						}
					},
					new ButtonMenuItem
					{
						Text = "&Wallet",
						Items =
						{
							
						}
					}
					,
					new ButtonMenuItem
					{
						Text = "&Advanced",
						Items =
						{
							
						}
					}
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
		}
	}
}
