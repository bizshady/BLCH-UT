using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using Nerva.Toolkit.CLI.Structures.Response;

namespace Nerva.Toolkit.Content
{	
	public partial class BalancesPage
	{
		private Scrollable mainControl;
        public Scrollable MainControl => mainControl;

		public BalancesPage() { }

        public void ConstructLayout()
		{
			mainControl = new Scrollable();
		}

		public void Update(Account a)
		{
			List<TableRow> rows = new List<TableRow>();

			if (a != null)
			{
				rows.Add(new TableRow(
					new TableCell(new Label { Text = "#" }),
					new TableCell(new Label { Text = "Label" }),
					new TableCell(new Label { Text = "Address" }),
					new TableCell(new Label { Text = "Balance" }),
					new TableCell(new Label { Text = "Unlocked" }),
					new TableCell(null, true),
					new TableCell(null)));

				foreach (var aa in a.Accounts)
				{
					rows.Add(new TableRow(
						new TableCell(new Label { Text = aa.Index.ToString() }),
						new TableCell(new Label { Text = aa.Label }),
						new TableCell(new Label { Text = Conversions.WalletAddressShortForm(aa.BaseAddress) }),
						new TableCell(new Label { Text = Conversions.FromAtomicUnits(aa.Balance).ToString() }),
						new TableCell(new Label { Text = Conversions.FromAtomicUnits(aa.UnlockedBalance).ToString() }),
						new TableCell(null, true),
						new TableCell(null)));
				}

				rows.Add(new TableRow(
						new TableCell(null),
						new TableCell(null),
						new TableCell(new Label { Text = "TOTAL:" }),
						new TableCell(new Label { Text = Conversions.FromAtomicUnits(a.TotalBalance).ToString() }),
						new TableCell(new Label { Text = Conversions.FromAtomicUnits(a.TotalUnlockedBalance).ToString() }),
						new TableCell(null, true),
						new TableCell(null)));
			}
			else
			{
				rows.Add(new TableRow(
					new TableCell(new Label { Text = "WALLET NOT OPEN" })));
			}

			mainControl.Content = new TableLayout(rows)
			{
				Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
			};
		}
    }
}