using System;
using System.Collections.Generic;
using Nerva.Toolkit.Helpers;
using System.Linq;
using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;

namespace Nerva.Toolkit.Content
{
    public partial class DaemonPage
	{
        List<string> la = new List<string>();

        #region Form Controls

        private StackLayout mainControl;
        public StackLayout MainControl => mainControl;

		TableLayout infoContainer;
		Scrollable connectionsContainer = new Scrollable();
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
            infoContainer = new TableLayout
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
			};

			connectionsContainer = new Scrollable
			{
				Content = grid = new GridView
				{
					GridLines = GridLines.Horizontal,
					Columns =
					{
						new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Connection, string>(r => r.Address)}, HeaderText = "Address" },
						new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Connection, string>(r => r.Height.ToString())}, HeaderText = "Height" },
						new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Connection, string>(r =>TimeSpan.FromSeconds(r.LiveTime).ToString(@"hh\:mm\:ss"))}, HeaderText = "Live Time" },
						new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Connection, string>(r => r.State.Remove(0, 6))}, HeaderText = "State" },
					}
				}
			};

			grid.SelectedRowsChanged += (s, e) =>
			{
				Connection c = (Connection)grid.DataStore.ElementAt(grid.SelectedRow);
			};

			mainControl = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				Items = 
				{
					new StackLayoutItem(infoContainer, false),
					new StackLayoutItem(new TableLayout
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
						Rows =
						{
							new TableRow (
								new TableCell(new Label { Text = "NERVA NETWORK CONNECTIONS" }))
						}
					}, false),
					new StackLayoutItem(connectionsContainer, true)
				}
			};
        }

        private void CreateConnectionsTable(List<Connection> connections)
		{
			/*List<TableRow> rows = new List<TableRow>();

			rows.Add(new TableRow(
				new TableCell(new Label { Text = "Connections" })));

			rows.Add(new TableRow(
				new TableCell(new Label { Text = "Address" }),
				new TableCell(new Label { Text = "Height" }),
				new TableCell(new Label { Text = "Live Time" }),
				new TableCell(new Label { Text = "State" }),
				new TableCell(null, true),
				new TableCell(null)));

			foreach (var c in connections)
			{
				Button btn = new Button { Text = "Ban" };
				btn.Click += (sender, e) => { Cli.Instance.Daemon.BanPeer(c.Host); };

				rows.Add(new TableRow(
					new TableCell(new Label { Text = c.Address }),
					new TableCell(new Label { Text = c.Height.ToString() }),
					new TableCell(new Label { Text = TimeSpan.FromSeconds(c.LiveTime).ToString(@"hh\:mm\:ss") }),
					new TableCell(new Label { Text = c.State.Remove(0, 6) }),
					new TableCell(null, true),
					TableLayout.AutoSized(btn)));
			}

			connectionsContainer.Content = new TableLayout(rows)
			{
				Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
			};*/

			//Testing the grid view as opposed to recreating the layout each time
			grid.DataStore = connections;

			//HACK to refresh grid
			//for (int i = 0; i < connections.Count; i++)
			//	grid.ReloadData(i);
		}

        public void Update(Info info, List<Connection> connections, MiningStatus mStatus)
        {
            if (info != null)
            {
                double nethash = Math.Round(((info.Difficulty / 60.0d) / 1000.0d), 2);
                //Update the daemon info
                lblHeight.Text = info.Height.ToString();
                lblNetHash.Text = nethash.ToString() + " kH/s";
                lblRunTime.Text = (DateTime.Now - Conversions.UnixTimeStampToDateTime((ulong)info.StartTime)).ToString(@"hh\:mm");

                if (info.Mainnet)
					lblNetwork.Text = "MainNet";
                else if (info.Testnet)
                    lblNetwork.Text = "TestNet";
                else
                    Log.Instance.Write(Log_Severity.Fatal, "Unknown network connection type");
            }
            else
            {
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

            if (connections != null)
            {
                //Check if we need to update the connections list
                /*List<string> a = connections.Select(x => x.Address).ToList();

                bool needUpdate = false;
                if (a.Count != la.Count)
                    needUpdate = true;

                if (!needUpdate)
                    for (int i = 0; i < a.Count; i++)
                        if (a[i] != la[i])
                        {
                            needUpdate = true;
                            break;
                        }

                if (needUpdate)
                {
                    mainControl.SuspendLayout();
                    
                    mainControl.ResumeLayout();
                    la = a;
				}*/

				CreateConnectionsTable(connections);
            }
            else
                connectionsContainer.Content = null;
        }
    }
}