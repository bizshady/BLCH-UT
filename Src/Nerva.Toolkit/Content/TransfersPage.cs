using System;
using System.Linq;
using System.Collections.Generic;
using Eto.Forms;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using AngryWasp.Helpers;
using Nerva.Toolkit.Content.Dialogs;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Config;
using Nerva.Rpc.Wallet;

namespace Nerva.Toolkit.Content
{
    public class TransfersPage
    {
        private Scrollable mainControl;
        public Scrollable MainControl => mainControl;

        GridView grid;
        List<TransferItem> txList = new List<TransferItem>();
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

                var t = txList[grid.SelectedRow];

                Helpers.TaskFactory.Instance.RunTask("gettx", $"Fetching transaction information", () =>
                {
                    var txid = Cli.Instance.Wallet.Interface.GetTransferByTxID(t.TxId);
                    if (txid != null)
                    {
                        Application.Instance.AsyncInvoke( () =>
                        {
                            ShowTxDialog d = new ShowTxDialog(txid);
                            d.ShowModal();
                        });
                    }
                    else
                    {
                        Application.Instance.AsyncInvoke( () =>
                        {
                            MessageBox.Show(Application.Instance.MainForm, "Transfer information could not be retrieved at this time", "Transaction Details", 
                                MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
                        });
                    }
                });
            };

            mainControl = new Scrollable();
            grid = new GridView
            {
                GridLines = GridLines.Horizontal,
                Columns =
                {
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<TransferItem, string>(r => r.Type)}, HeaderText = "Type" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<TransferItem, string>(r => r.Height.ToString())}, HeaderText = "Height" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<TransferItem, string>(r => StringHelper.UnixTimeStampToDateTime(r.Timestamp).ToString())}, HeaderText = "Time" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<TransferItem, string>(r => Conversions.FromAtomicUnits(r.Amount).ToString())}, HeaderText = "Amount" },
                    new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<TransferItem, string>(r => Conversions.WalletAddressShortForm(r.TxId))}, HeaderText = "TxID" },
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

        public void Update(GetTransfersResponseData t)
        {
            try
            {
                int rowsAdded = ProcessNewTransfers(t);

                if (needGridUpdate)
                {
                    int newSelectedRow = CalculateNewHighlightedRow(rowsAdded);

                    if (txList.Count == 0)
                        grid.DataStore = null;
                    else
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
                {
                    mainControl.Content = new TableLayout(new TableRow(
                        new TableCell(new Label { Text = "NO TRANSFERS" })))
                    {
                        Padding = 10,
                        Spacing = new Eto.Drawing.Size(10, 10),
                    };
                }
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

        private int ProcessNewTransfers(GetTransfersResponseData t)
        {
			int i = 0;

			if (t == null)
			{
				txList.Clear();
                needGridUpdate = true;
				return -1;
			}

            var merged = new List<TransferItem>();
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

            if (i > 0)
                Cli.Instance.Wallet.Interface.Store();

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