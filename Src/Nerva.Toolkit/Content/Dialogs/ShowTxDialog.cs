using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class ShowTxDialog : Dialog
	{
        Button btnOk = new Button { Text = "OK" };

        public ShowTxDialog(TransferTxID tx)
        {
            this.Title = "Transaction Details";
            ClientSize = new Size(400, 100);
            Topmost = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            CreateLayout(tx);

            this.DefaultButton = btnOk;
        }

        public void CreateLayout(TransferTxID tx)
        {
            List<TableRow> rows = new List<TableRow>();

			foreach (var d in tx.Destinations)
				rows.Add(new TableRow (
					new TableCell(new Label { Text = d.Address }),
					new TableCell(new Label { Text = Conversions.FromAtomicUnits(d.Amount).ToString() }, true)));
            
            Content = new StackLayout
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
							new TableRow (
								new TableCell(new Label { Text = "Address:" }),
								new TableCell(new Label { Text = tx.Address }, true)),
							new TableRow (
								new TableCell(new Label { Text = "TX:" }),
								new TableCell(new Label { Text = tx.TxId }, true)),
                            new TableRow (
								new TableCell(new Label { Text = "ID:" }),
								new TableCell(new Label { Text = tx.PaymentId }, true)),
							new TableRow (
								new TableCell(new Label { Text = "Note:" }),
								new TableCell(new Label { Text = tx.Note }, true))
                        }
                    }),
					new StackLayoutItem(new TableLayout
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
						Rows =
						{
							new TableRow (
								new TableCell(new Label { Text = "Type:" }),
								new TableCell(new Label { Text = tx.Type }, true),
								new TableCell(new Label { Text = "Time:" }),
								new TableCell(new Label { Text = Conversions.UnixTimeStampToDateTime(tx.Timestamp).ToString() }, true)),
							new TableRow (
								new TableCell(new Label { Text = "Amount:" }),
								new TableCell(new Label { Text = Conversions.FromAtomicUnits(tx.Amount).ToString() }, true),
								new TableCell(new Label { Text = "Fee:" }),
								new TableCell(new Label { Text = Conversions.FromAtomicUnits(tx.Fee).ToString() }, true)),
							new TableRow(
								new TableCell(new Label { Text = "Height:" }),
								new TableCell(new Label { Text = tx.Height.ToString() }),
								new TableCell(new Label { Text = "Unlock Time:" }),
								new TableCell(new Label { Text = tx.UnlockTime.ToString() })),
							new TableRow(
								new TableCell(new Label { Text = "Index:" }),
								new TableCell(new Label { Text = $"{tx.SubAddressIndex.Major}.{tx.SubAddressIndex.Minor}" }),
								new TableCell(new Label { Text = "Double Spend:" }),
								new TableCell(new Label { Text = tx.DoubleSpendSeen.ToString() })),
							new TableRow(
								new TableCell(null),
								new TableCell(null))
						}
					}, false),
					new StackLayoutItem(new TableLayout(rows), true)
				}
			};
        }
    }
}