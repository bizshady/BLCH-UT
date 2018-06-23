using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;
using Nerva.Toolkit.Helpers;
using Nerva.Toolkit.CLI.Structures.Response;

namespace Nerva.Toolkit.Content
{	
	public partial class WalletPage
	{
		private StackLayout mainControl;
        public StackLayout MainControl => mainControl;

		TableLayout infoContainer;
		Scrollable transactionsContainer = new Scrollable();

		private Label lblWalletAddress = new Label { Text = "NVxxxx...xxxxxx" };
		private Label lblLockedBalance = new Label { Text = "0.0000" };
		private Label lblUnlockedBalance = new Label { Text = "0.0000" };

		public WalletPage() { }

        public void ConstructLayout()
		{
			infoContainer = new TableLayout
			{
				Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
				Rows =
				{
					new TableRow (
						new TableCell(new Label { Text = "Wallet:" }),
						new TableCell(lblWalletAddress, true)),
					new TableRow (
						new TableCell(new Label { Text = "Locked Balance:" }),
						new TableCell(lblLockedBalance, true)),
					new TableRow (
						new TableCell(new Label { Text = "Unlocked Balance:" }),
						new TableCell(lblUnlockedBalance, true)),	
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
					new StackLayoutItem(transactionsContainer, true)
				}
			};
		}

		public void Update(Account a)
		{
			if (a != null)
			{
				lblWalletAddress.Text = Conversions.WalletAddressShortForm(a.Accounts[0].BaseAddress);
				lblLockedBalance.Text = a.Accounts[0].Balance.ToString();
				lblUnlockedBalance.Text = a.Accounts[0].UnlockedBalance.ToString();
			}
			else
			{
				lblWalletAddress.Text = "WALLET NOT OPEN";
				lblLockedBalance.Text = "-";
				lblUnlockedBalance.Text = "-";
			}
		}
    }
}