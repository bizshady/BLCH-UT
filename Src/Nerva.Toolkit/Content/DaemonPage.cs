using System;
using System.Collections.Generic;
using Nerva.Toolkit.Helpers;
using System.Linq;
using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using AngryWasp.Helpers;
using System.Diagnostics;
using Nerva.Rpc.Daemon;

namespace Nerva.Toolkit.Content
{
    public class DaemonPage
	{
        List<string> la = new List<string>();

        #region Form Controls

        private StackLayout mainControl;
        public StackLayout MainControl => mainControl;

		GridView grid;
	
		private Label lblHeight = new Label() { Text = "." };
		private Label lblRunTime = new Label() { Text = "." };
		private Label lblNetHash = new Label() { Text = "." };
		private Label lblNetwork = new Label() { Text = "." };

		private Label lblMinerStatus = new Label { Text = "Miner (Inactive)" };
		private Label lblMiningAddress = new Label() { Text = "." };
		private Label lblMiningThreads = new Label() { Text = "." };
		private Label lblMiningHashrate = new Label() { Text = "." };

        #endregion

        public DaemonPage() { }

        public void ConstructLayout()
        {
			var peersCtx_Ban = new Command { MenuText = "Ban Peer" };

			grid = new GridView
			{
				GridLines = GridLines.Horizontal,
				Columns = 
				{
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<GetConnectionsResponseData, string>(r => r.Address)}, HeaderText = "Address" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<GetConnectionsResponseData, string>(r => r.Height.ToString())}, HeaderText = "Height" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<GetConnectionsResponseData, string>(r => TimeSpan.FromSeconds(r.LiveTime).ToString(@"hh\:mm\:ss"))}, HeaderText = "Live Time" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<GetConnectionsResponseData, string>(r => r.State)}, HeaderText = "State" }
				}
			};

			grid.ContextMenu = new ContextMenu
			{
				Items = 
				{
					peersCtx_Ban
				}
			};

			peersCtx_Ban.Executed += (s, e) =>
			{
				if (grid.SelectedRow == -1)
					return;

				GetConnectionsResponseData c = (GetConnectionsResponseData)grid.DataStore.ElementAt(grid.SelectedRow);
				Cli.Instance.Daemon.Interface.BanPeer(c.IP);
			};

			mainControl = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				Items = 
				{
					new StackLayoutItem(new TableLayout
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
						Rows =
						{
							new TableRow (
								new TableCell(new Label { Text = "Daemon" }),
								new TableCell(null, true),
								new TableCell(lblMinerStatus),
								new TableCell(null, true)),
							new TableRow(
								new TableCell(new Label { Text = "Height:" }),
								new TableCell(lblHeight),
								new TableCell(new Label { Text = "Address:" }),
								new TableCell(lblMiningAddress)),
							new TableRow(
								new TableCell(new Label { Text = "Run Time:" }),
								new TableCell(lblRunTime),
								new TableCell(new Label { Text = "Threads:" }),
								new TableCell(lblMiningThreads)),
							new TableRow(
								new TableCell(new Label { Text = "Net Hash:" }),
								new TableCell(lblNetHash),
								new TableCell(new Label { Text = "Hash Rate:" }),
								new TableCell(lblMiningHashrate)),
							new TableRow(
								new TableCell(new Label { Text = "Network:" }),
								new TableCell(lblNetwork)),
							new TableRow(
								new TableCell(null),
								new TableCell(null))
						}
					}, false),
					new StackLayoutItem(new Scrollable
					{
						Content = grid
					}, true)
				}
			};
        }

        public void Update(GetInfoResponseData info, List<GetConnectionsResponseData> connections, MiningStatusResponseData mStatus)
        {
			try
			{
				if (info != null)
				{
					double nethash = Math.Round(((info.Difficulty / 60.0d) / 1000.0d), 2);
					//Update the daemon info
					lblHeight.Text = info.Height.ToString();
					lblNetHash.Text = nethash.ToString() + " kH/s";
					lblRunTime.Text = (DateTime.Now - StringHelper.UnixTimeStampToDateTime((ulong)info.StartTime)).ToString(@"hh\:mm");

					if (info.Mainnet)
						lblNetwork.Text = "MainNet";
					else if (info.Testnet)
						lblNetwork.Text = "TestNet";
					else
						Log.Instance.Write(Log_Severity.Fatal, "Unknown network connection type");
				}
				else
				{
					lblNetwork.Text = "-";
					lblHeight.Text = "-";
					lblNetHash.Text = "-";
					lblRunTime.Text = "-";
				}

				if (mStatus != null && mStatus.Active)
				{
					lblMinerStatus.Text = "Miner (Active)";
					lblMiningAddress.Text = Conversions.WalletAddressShortForm(mStatus.Address);
					lblMiningThreads.Text = mStatus.ThreadCount.ToString();

					string speed;
					if (mStatus.Speed > 1000)
						speed = $"{mStatus.Speed / 1000.0d} kH/s";
					else
						speed = $"{(double)mStatus.Speed} h/s";
					
					lblMiningHashrate.Text = speed;
				}
				else
				{
					lblMinerStatus.Text = "Miner (Inactive)";
					lblMiningAddress.Text = "-";
					lblMiningThreads.Text = "-";
					lblMiningHashrate.Text = "-";
				}

				if (connections == null)
					connections = new List<GetConnectionsResponseData>();

				if (OS.Type == OS_Type.Windows)
				{
					int si = grid.SelectedRow;
					grid.DataStore = connections;
					grid.SelectRow(si);
				}
				else
					grid.DataStore = connections;
			}
			catch (Exception ex)
			{
				Log.Instance.WriteNonFatalException(ex);
			}
        }
    }
}