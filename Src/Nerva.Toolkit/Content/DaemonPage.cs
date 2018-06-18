using System;
using System.Collections.Generic;
using Nerva.Toolkit.CLI.Structures;
using Nerva.Toolkit.Helpers;
using System.Linq;
using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;

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
	
		private Label lblHeight = new Label() { Text = "." };
		private Label lblRunTime = new Label() { Text = "." };
		private Label lblNetHash = new Label() { Text = "." };
		private Label lblNetwork = new Label() { Text = "." };

		private Label lblMinerStatus = new Label { Text = "Miner" };
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
				}
			};

			mainControl = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				Items = 
				{
					new StackLayoutItem(infoContainer, false),
					new StackLayoutItem(connectionsContainer, true)
				}
			};
        }

        private void CreateConnectionsTable(List<Connection> connections)
		{
			List<TableRow> rows = new List<TableRow>();

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
			};
		}

        public void Update(Info info, List<Connection> connections, MiningStatus mStatus)
        {
            if (info != null)
            {
                double nethash = Math.Round(((info.Difficulty / 60.0d) / 1000.0d), 2);
                //Update the daemon info
                lblHeight.Text = info.Height.ToString();
                lblNetHash.Text = nethash.ToString() + " kH/s";
                lblRunTime.Text = (DateTime.Now - Conversions.UnixTimeStampToDateTime((ulong)info.StartTime)).ToString(@"hh\:mm\:ss");

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
                lblMiningAddress.Text = mStatus.Address.Substring(0, 6) + "..." + mStatus.Address.Substring(mStatus.Address.Length - 6, 6);
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
                List<string> a = connections.Select(x => x.Address).ToList();

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
                    CreateConnectionsTable(connections);
                    mainControl.ResumeLayout();
                    la = a;
                }
            }
            else
            {
                connectionsContainer.Content = null;
            }
        }
    }
}