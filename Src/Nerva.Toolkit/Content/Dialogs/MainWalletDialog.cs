using AngryWasp.Logger;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Dialogs
{
    public enum Open_Wallet_Dialog_Result
    {
        Cancel,
        New,
        Import,
        Open
    }

    public class MainWalletDialog : Dialog<Open_Wallet_Dialog_Result>
	{
        Button btnOpenWallet = new Button { Text = "Open" };
        Button btnImportWallet = new Button { Text = "Import" };
        Button btnNewWallet = new Button { Text = "New" };
        Button btnCancel = new Button { Text = "Cancel" };

        public MainWalletDialog()
        {
            this.Title = "Open/Import Wallet";
            Topmost = true;
            this.Resizable = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            CreateLayout();

            this.AbortButton = btnCancel;

            btnNewWallet.Click += (s, e) => Close(Open_Wallet_Dialog_Result.New);
            btnOpenWallet.Click += (s, e) => Close(Open_Wallet_Dialog_Result.Open);
            btnImportWallet.Click += (s, e) =>Close(Open_Wallet_Dialog_Result.Import);
            btnCancel.Click += (s, e) => Close(Open_Wallet_Dialog_Result.Cancel);

            if (WalletHelper.GetWalletFileCount() == 0)
            {
                btnOpenWallet.Enabled = false;
                this.DefaultButton = btnNewWallet;
            }
            else
                this.DefaultButton = btnOpenWallet;

            this.Focus();
        }

        public void CreateLayout()
        {
            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center,
                Items = 
                {
                    new StackLayoutItem(null, true),
                    new StackLayoutItem(btnOpenWallet),
                    new StackLayoutItem(btnNewWallet),
                    new StackLayoutItem(btnImportWallet),
                    new StackLayoutItem(btnCancel),
                    new StackLayoutItem(null, true),
                }
            };
        }
    }
}