using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class OpenWalletDialog : Dialog<DialogResult>
	{
        bool isShown = false;

        private string name;
        private string password;

        public string Password => password;
        public string Name => name;

        PasswordBox pwb = new PasswordBox { PasswordChar = '*' };
        TextBox tb = new TextBox();

        DropDown ddName = new DropDown();

        TableRow pwr = new TableRow();

        Button btnShow = new Button { Text = "Show" };
        Button btnOk = new Button { Text = "OK" };
        Button btnCancel = new Button { Text = "Cancel" };

        public OpenWalletDialog()
        {
            this.Title = "Open Wallet";
            this.Width = 400;
            Topmost = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            CreateLayout();

            this.AbortButton = btnCancel;
            this.DefaultButton = btnOk;

            btnShow.Click += (s, e) =>
            {
                isShown = !isShown;
                if (isShown)
                    tb.Text = pwb.Text;
                else
                    pwb.Text = tb.Text;

                CreateLayout();
            };

            btnOk.Click += (s, e) =>
            {
                if (isShown)
                    password = tb.Text;
                else
                    password = pwb.Text;

                name = ddName.SelectedValue.ToString();

                bool opened = Cli.Instance.Wallet.OpenWallet(name, password);

                if (opened)
                    this.Close(DialogResult.Ok);
                else
                {
                    MessageBox.Show(this, "Could not open wallet", "Open wallet failed", MessageBoxButtons.OK, MessageBoxType.Error, MessageBoxDefaultButton.OK);
                    tb.Text = null;
                    pwb.Text = null;
                }
            };

            btnCancel.Click += (s, e) =>
            {
                password = null;
                name = null;
                this.Close(DialogResult.Cancel);
            };
        }

        public void CreateLayout()
        {
            TextControl textControl;
            
            if (isShown)
                textControl = tb;
            else
                textControl = pwb;

            if (ddName.Items.Count == 0)
                foreach (var f in WalletHelper.GetWalletFiles())
                    ddName.Items.Add(Path.GetFileNameWithoutExtension(f.FullName));

            ddName.SelectedIndex = 0;

            Content = new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    new Label { Text = "Wallet Name" },
                    ddName,
                    new Label { Text = "Password" },
                    textControl,
                    new TableRow (
                        new TableLayout
                        {
                            Rows = {
                                new TableRow (
                                    btnShow,
                                    new TableCell(null, true),
                                    btnOk,
                                    btnCancel)
                            }
                        }),
                    new TableRow { ScaleHeight = true }
                }
            };
        }
    }
}