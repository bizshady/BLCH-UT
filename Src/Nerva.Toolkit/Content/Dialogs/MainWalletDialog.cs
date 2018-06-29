using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;

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
            this.Width = 400;
            Topmost = true;
            this.Resizable = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            CreateLayout();

            this.AbortButton = btnCancel;

            btnNewWallet.Click += (s, e) =>  { Close(Open_Wallet_Dialog_Result.New); };
            btnOpenWallet.Click += (s, e) =>  { Close(Open_Wallet_Dialog_Result.Open); };
            btnImportWallet.Click += (s, e) => { Close(Open_Wallet_Dialog_Result.Import); };
            btnCancel.Click += (s, e) => { Close(Open_Wallet_Dialog_Result.Cancel); };

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
            Content = new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    new TableRow (
                        btnOpenWallet,
                        btnNewWallet,
                        btnImportWallet,
                        btnCancel)}
            };
        }
    }
}