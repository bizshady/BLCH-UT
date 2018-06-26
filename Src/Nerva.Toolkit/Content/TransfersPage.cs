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

		GridView grid;
		List<Transfer> txList = new List<Transfer>();

		public TransfersPage() { }

        public void ConstructLayout()
		{
			mainControl = new Scrollable();
			grid = new GridView
			{
				GridLines = GridLines.Horizontal,
				Columns =
				{
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => r.Type)}, HeaderText = "Type" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => r.Height.ToString())}, HeaderText = "Height" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => Conversions.UnixTimeStampToDateTime(r.Timestamp).ToString())}, HeaderText = "Time" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => Conversions.FromAtomicUnits(r.Amount).ToString())}, HeaderText = "Amount" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => Conversions.WalletAddressShortForm(r.TxId))}, HeaderText = "TxID" },
				}
			};
		}

		//Dictionary<uint, TableRow> rows = new Dictionary<uint, TableRow>();

		public void Update(TransferList t)
		{
			if (t != null)
			{
				List<Transfer> merged = new List<Transfer>();
				merged.AddRange(t.Incoming);
				merged.AddRange(t.Outgoing);
				merged.AddRange(t.Pending);

				if (merged.Count > 0)
				{
					//descending order by height and get top 50
					merged = merged.OrderByDescending(x => x.Height).Take(50).ToList();

					//HACK: The RPC request should only return the full transfer list the first time
					//then only new ones after that, by using the 'filter_by_height' and 'min_height'
					//RPC params. These appear to be not working. Needs investigation
					//Until then, we just check if the top transfer is a higher block and then refresh if it is
					//TODO: if we can't find out why filtering isn't working properly, then we need to prune back
					//the merged list until it only contains new transfers. then update the selected row
					//in the grid after inserting the new rows
					if (txList.Count == 0 || (merged.Count > 0 && merged[0].Height > txList[0].Height))
					{
						//insert into our list of existing transfers and trim back to 50
						txList.InsertRange(0, merged);
						txList = txList.Take(50).ToList();

						grid.DataStore = txList;
						mainControl.Content = grid;
					}
				}
			}
			else
			{
				mainControl.Content = new TableLayout(new TableRow(
					new TableCell(new Label { Text = "WALLET NOT OPEN" })))
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
					};
			}
			//todo: get top n from each list, merge, sort, trim back to top n
			//make n a config option
			/*if (t != null)
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

			}*/
		}
    }
}