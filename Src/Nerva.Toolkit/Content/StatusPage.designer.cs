using System;
using System.Collections.Generic;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures;

namespace Nerva.Toolkit.Content
{
    public partial class StatusPage : ITabPageContent
	{
		private StackLayout mainControl;
		TableLayout infoContainer;
		Scrollable connectionsContainer = new Scrollable();
		public Control MainControl => mainControl;

		private Label lblHeight = new Label() { Text = "." };
		private Label lblRunTime = new Label() { Text = "." };
		private Label lblNetHash = new Label() { Text = "." };
		private Label lblNetwork = new Label() { Text = "." };

		private Label lblMinerStatus = new Label { Text = "Miner" };
		private Label lblMiningAddress = new Label() { Text = "." };
		private Label lblMiningThreads = new Label() { Text = "." };
		private Label lblMiningHashrate = new Label() { Text = "." };

		public StatusPage()
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
    }
}