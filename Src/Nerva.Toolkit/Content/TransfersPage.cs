using System;
using System.Linq;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using Nerva.Toolkit.CLI.Structures.Response;
using AngryWasp.Helpers;
using Nerva.Toolkit.Content.Dialogs;
using Nerva.Toolkit.CLI;
using System.Diagnostics;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Content
{
    public class TransfersPage
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
            var ctx_TxDetails = new Command { MenuText = "Details" };

            ctx_TxDetails.Executed += (s, e) =>
            {
                if (grid.SelectedRow == -1)
                    return;

                Transfer t = txList[grid.SelectedRow];
                ShowTxDialog d = new ShowTxDialog(Cli.Instance.Wallet.Interface.GetTransferByTxID(t.TxId));
                d.ShowModal();
            };

            mainControl = new Scrollable();
            grid = new GridView
            {
                GridLines = GridLines.Horizontal,
                Columns =
                {
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => r.Type)}, HeaderText = "Type" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => r.Height.ToString())}, HeaderText = "Height" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => StringHelper.UnixTimeStampToDateTime(r.Timestamp).ToString())}, HeaderText = "Time" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => Conversions.FromAtomicUnits(r.Amount).ToString())}, HeaderText = "Amount" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<Transfer, string>(r => Conversions.WalletAddressShortForm(r.TxId))}, HeaderText = "TxID" },
                }
            };

            grid.ContextMenu = new ContextMenu
            {
                Items =
                {
                    ctx_TxDetails
                }
            };

            Update(null);
        }

        public void Update(TransferList t)
        {
            try
            {
                int rowsAdded = ProcessNewTransfers(t);

                if (needGridUpdate)
                {
                    int newSelectedRow = CalculateNewHighlightedRow(rowsAdded);

                    grid.DataStore = txList;

                    if (newSelectedRow != -1)
                    {
                        grid.SelectedRow = newSelectedRow;
                        grid.ScrollToRow(grid.SelectedRow);
                    }

                    needGridUpdate = false;
                    lastHeight = txList.Count == 0 ? 0 : txList[0].Height;
                }

                if (txList.Count == 0)
                    mainControl.Content = new TableLayout(new TableRow(
                        new TableCell(new Label { Text = "NO TRANSFERS" })))
                    {
                        Padding = 10,
                        Spacing = new Eto.Drawing.Size(10, 10),
                    };
                else
				{
					if (mainControl.Content != grid)
                    	mainControl.Content = grid;
				}
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNonFatalException(ex);
            }
        }

        private int ProcessNewTransfers(TransferList t)
        {
			int i = 0;

			if (t == null)
			{
				txList.Clear();
                needGridUpdate = true;
				return -1;
			}

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

                        if (i >= merged.Count)
                            break;
                    }

                    if (i > 0)
                    {
                        txList.InsertRange(0, merged.GetRange(0, i));
                        needGridUpdate = true;
                    }
                }
            }

			int maxRows = Configuration.Instance.Wallet.NumTransfersToDisplay;

            if (txList.Count > maxRows)
                txList = txList.Take(maxRows).ToList();

			return i;
        }

		private int CalculateNewHighlightedRow(int i)
		{
			if (txList.Count == 0)
				return -1;

			if (grid.SelectedRow == -1)
				return -1;

			int x = grid.SelectedRow + i;
            MathHelper.Clamp(ref x, 0, Configuration.Instance.Wallet.NumTransfersToDisplay - 1);

			return x;
		}
    }
}