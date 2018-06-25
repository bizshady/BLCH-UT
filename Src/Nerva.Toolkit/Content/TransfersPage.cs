using System;
using System.Linq;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using Nerva.Toolkit.CLI.Structures.Response;

namespace Nerva.Toolkit.Content
{	
	public partial class TransfersPage
	{
		private Scrollable mainControl;
        public Scrollable MainControl => mainControl;

		private int lastRowCount = -1;

		public TransfersPage() { }

        public void ConstructLayout()
		{
			mainControl = new Scrollable();
		}

		Dictionary<uint, TableRow> rows = new Dictionary<uint, TableRow>();

		public void Update(TransferList t)
		{
			if (t != null)
			{
				if (!rows.ContainsKey(0))
					rows.Add(0, new TableRow(
						new TableCell(new Label { Text = "Height" }),
						new TableCell(new Label { Text = "Time" }),
						new TableCell(new Label { Text = "Amount" }),
						new TableCell(new Label { Text = "TxID" }),
						new TableCell(null, true),
						new TableCell(null)));

				foreach (var i in t.Incoming)
				{
					if (rows.ContainsKey(i.Height))
						continue;

					rows.Add(i.Height, new TableRow(
						new TableCell(new Label { Text = i.Height.ToString() }),
						new TableCell(new Label { Text = Conversions.UnixTimeStampToDateTime(i.Timestamp).ToString() }),
						new TableCell(new Label { Text = Conversions.FromAtomicUnits(i.Amount).ToString() }),
						new TableCell(new Label { Text = Conversions.WalletAddressShortForm(i.TxId) }),
						new TableCell(null, true),
						new TableCell(new Button { Text = "Details"} )));
				}

				if (rows.Count != lastRowCount)
				{
					lastRowCount = rows.Count;
					var sortedRows = rows.OrderByDescending(x => x.Key).Take(20).ToDictionary(pair => pair.Key, pair => pair.Value);
					mainControl.Content = new TableLayout(sortedRows.Values)
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
					};
				}
			}
			else
			{
				lastRowCount = -1;
				mainControl.Content = new TableLayout(new TableRow(
					new TableCell(new Label { Text = "WALLET NOT OPEN" })))
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
					};
			}
		}
    }
}