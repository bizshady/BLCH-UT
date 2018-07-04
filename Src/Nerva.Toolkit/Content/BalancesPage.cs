using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Content.Dialogs;
using System.Diagnostics;

namespace Nerva.Toolkit.Content
{	
	public class BalancesPage
	{
		private StackLayout mainControl;
        public StackLayout MainControl => mainControl;

		GridView grid;

		Label lblTotalXnv = new Label();
		Label lblUnlockedXnv = new Label();

		private List<SubAddressAccount> accounts = new List<SubAddressAccount>();

		public BalancesPage() { }

        public void ConstructLayout()
		{
			var ctx_Mine = new Command { MenuText = "Mine" };
			var ctx_Transfer = new Command { MenuText = "Transfer" };
			var ctx_Rename = new Command { MenuText = "Rename" };
			var ctx_Info = new Command { MenuText = "Address" };

			ctx_Mine.Executed += (s, e) =>
			{
				if (grid.SelectedRow == -1)
					return;

				SubAddressAccount a = accounts[grid.SelectedRow];
				Configuration.Instance.Daemon.MiningAddress = a.BaseAddress;
				Configuration.Save();

				Cli.Instance.Daemon.StopMining();
				Log.Instance.Write("Mining stopped");

				if (Cli.Instance.Daemon.StartMining(Configuration.Instance.Daemon.MiningThreads))
					Log.Instance.Write("Mining started for @ {0} on {1} threads", 
						Conversions.WalletAddressShortForm(Configuration.Instance.Daemon.MiningAddress),
						Configuration.Instance.Daemon.MiningThreads);
			};

			ctx_Info.Executed += (s, e) =>
			{
				if (grid.SelectedRow == -1)
					return;

				SubAddressAccount a = accounts[grid.SelectedRow];

				string lbl = string.IsNullOrEmpty(a.Label) ? "No Label" : a.Label;

				TextDialog d = new TextDialog($"Address for account '{lbl}'", true, a.BaseAddress);
				d.ShowModal();
			};

			ctx_Transfer.Executed += (s, e) =>
			{
				if (grid.SelectedRow == -1)
					return;

				SubAddressAccount a = accounts[grid.SelectedRow];

				TransferDialog d = new TransferDialog(a);
				if (d.ShowModal() == DialogResult.Ok)
				{
					if (d.TxData != null)
					{
						var x = d.TxData;
						string amt = Conversions.FromAtomicUnits(x.Amount).ToString();
						string fee = Conversions.FromAtomicUnits(x.Fee).ToString();
						string txh = x.TxHash;
						string txk = x.TxKey;

						string fmt = $"Sent: {amt}\r\nFees: {fee}\r\nHash: {txh}";

						MessageBox.Show(Application.Instance.MainForm, fmt, "TX Results", MessageBoxType.Information);
					}
					else
						Log.Instance.Write(Log_Severity.Fatal, "Failed to create transaction");
				}
			};

			ctx_Rename.Executed += (s, e) =>
			{
				if (grid.SelectedRow == -1)
					return;

				TextDialog d = new TextDialog("Select Account Name", false);

				if (d.ShowModal() == DialogResult.Ok)
					if (!Cli.Instance.Wallet.LabelAccount((uint)grid.SelectedRow, d.Text))
						MessageBox.Show(this.MainControl, "Failed to rename account", "Wallet rename",
                    		MessageBoxButtons.OK, MessageBoxType.Error, MessageBoxDefaultButton.OK);
			};

			ContextMenu ctx = new ContextMenu
			{
				Items = 
				{
					ctx_Info,
					ctx_Mine,
					ctx_Transfer,
					ctx_Rename
				}
			};

			grid = new GridView
			{
				GridLines = GridLines.Horizontal,
				Columns = 
				{
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => r.Index.ToString())}, HeaderText = "#" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => r.Label)}, HeaderText = "Label" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => Conversions.WalletAddressShortForm(r.BaseAddress))}, HeaderText = "Address" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => Conversions.FromAtomicUnits(r.Balance).ToString())}, HeaderText = "Balance" },
					new GridColumn { DataCell = new TextBoxCell { Binding = Binding.Property<SubAddressAccount, string>(r => Conversions.FromAtomicUnits(r.UnlockedBalance).ToString())}, HeaderText = "Unlocked" },
				}
			};

			grid.MouseDown += (s, e) =>
			{
				var cell = grid.GetCellAt(e.Location);
				if (cell.RowIndex == -1)
				{
					grid.UnselectAll();
					return;
				}

				if (e.Buttons != MouseButtons.Alternate)
					return;

				if (grid.SelectedRow == -1)
					return;

				ctx.Show(grid);
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
							new TableRow(
								new TableCell(new Label { Text = "Total XNV" }),
								new TableCell(lblTotalXnv, true),
								new TableCell(null)),
							new TableRow(
								new TableCell(new Label { Text = "Unlocked XNV" }),
								new TableCell(lblUnlockedXnv, true),
								new TableCell(null))
						}
					}, false),
					new StackLayoutItem(grid, true)
				}
			};
		}

		public void Update(Account a)
		{
			if (a != null)
			{
				lblTotalXnv.Text = Conversions.FromAtomicUnits(a.TotalBalance).ToString();
				lblUnlockedXnv.Text = Conversions.FromAtomicUnits(a.TotalUnlockedBalance).ToString();
				accounts = a.Accounts;
			}
			else
			{
				lblTotalXnv.Text = "-";
				lblUnlockedXnv.Text = "-";
				accounts.Clear();
			}

			if (OS.Type == OS_Type.Windows)
			{
				int si = grid.SelectedRow;
				grid.DataStore = accounts;
				grid.SelectRow(si);
			}
			else
				grid.DataStore = accounts;
		}
    }
}