using System;
using System.Linq;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using Nerva.Toolkit.CLI.Structures.Response;
using AngryWasp.Helpers;

namespace Nerva.Toolkit.Content
{	
	public partial class TransfersPage
	{
		private Scrollable mainControl;
        public Scrollable MainControl => mainControl;

		GridView grid;
		List<Transfer> txList = new List<Transfer>();
		bool needGridUpdate = false;
		uint lastHeight = 0;

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

		public void Update(TransferList t)
		{
			if (t != null)
			{
				int i = 0;
				List<Transfer> merged = new List<Transfer>();
				merged.AddRange(t.Incoming);
				merged.AddRange(t.Outgoing);
				merged.AddRange(t.Pending);

				if (merged.Count > 0)
				{
					//descending order by height and get top 50
					merged = merged.OrderByDescending(x => x.Height).ToList();

					if (txList.Count == 0)
					{
						txList = merged;
						needGridUpdate = true;
					}
					else
					{
						uint height = 0;

						while ((height = merged[i].Height) > lastHeight)
						{
							++i;
							Log.Instance.Write("Found TX on block {0}", height);
						}

						if (i > 0)
						{
							txList.InsertRange(0, merged.GetRange(0, i));
							needGridUpdate = true;
						}
					}

					if (needGridUpdate)
					{
						//todo: make the number of transactions to show a setting
						int maxRows = 25;
						
						txList = txList.Take(maxRows).ToList();
						grid.DataStore = txList;

						//update the selected row in the grid
						if (grid.SelectedRow != -1)
						{
							int newSelectedRow = grid.SelectedRow + i;
							MathHelper.Clamp(ref newSelectedRow, 0, maxRows - 1);
							grid.SelectedRow = newSelectedRow;
							grid.ScrollToRow(grid.SelectedRow);
						}

						mainControl.Content = grid;
						needGridUpdate = false;
						lastHeight = txList[0].Height;
					}
				}
			}
			else
			{
				needGridUpdate = true;
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