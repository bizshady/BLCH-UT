using System;
using System.Collections.Generic;
using Eto.Forms;
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
						new TableCell(new Label { Text = "Daemon Status" }),
						new TableCell(null),
						new TableCell(new Label { Text = "Miner Status" }),
						new TableCell(null)),
					new TableRow(
						new TableCell(new Label { Text = "Height:" }),
						new TableCell(lblHeight, true),
						new TableCell(new Label { Text = "Address:" }),
						new TableCell(lblMiningAddress, true)),
					new TableRow(
						new TableCell(new Label { Text = "Run Time:" }),
						new TableCell(lblRunTime, true),
						new TableCell(new Label { Text = "Threads:" }),
						new TableCell(lblMiningThreads, true)),
					new TableRow(
						new TableCell(new Label { Text = "Net Hash:" }),
						new TableCell(lblNetHash, true),
						new TableCell(new Label { Text = "Hash Rate:" }),
						new TableCell(lblMiningHashrate, true)),
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
				rows.Add(new TableRow(
					new TableCell(new Label { Text = c.Address }),
					new TableCell(new Label { Text = c.Height.ToString() }),
					new TableCell(new Label { Text = TimeSpan.FromSeconds(c.LiveTime).ToString(@"hh\:mm\:ss") }),
					new TableCell(new Label { Text = c.State.Remove(0, 6) }),
					new TableCell(null, true),
					TableLayout.AutoSized(new Button { Text = "Ban" })));

			connectionsContainer.Content = new TableLayout(rows)
			{
				Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
			};
		}
    }
}