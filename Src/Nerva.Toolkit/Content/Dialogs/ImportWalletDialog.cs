using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class ImportWalletDialog : Dialog<DialogResult>
	{
        bool isShown = false;

        private string name;
        private string password;

        public string Password => password;
        public string Name => name;

        PasswordBox pwb = new PasswordBox { PasswordChar = '*' };
        TextBox tb = new TextBox();

        TextBox txtName = new TextBox();

        TableRow pwr = new TableRow();

        TextArea txtKey = new TextArea { Wrap = true };

        Button btnShow = new Button { Text = "Show" };
        Button btnOk = new Button { Text = "OK" };
        Button btnCancel = new Button { Text = "Cancel" };

        public ImportWalletDialog()
        {
            this.Title = "Import Wallet";
            this.Width = 400;
            this.Resizable = true;
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

                name = txtName.Text;

                BackgroundWorker w = new BackgroundWorker();

                w.DoWork += (ws, we) =>
                {
                    Cli.Instance.Wallet.RestoreWalletFromSeed(name, txtKey.Text, password);
                };

                w.RunWorkerCompleted += (ws, we) =>
                {
                    MessageBox.Show("wallet import complete");
                };

                w.RunWorkerAsync();
                this.Close(DialogResult.Ok);
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

            Content = new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    new Label { Text = "Mnemonic Seed" },
                    new TableRow(txtKey)
                    {
                        ScaleHeight = true
                    },
                    new Label { Text = "Wallet Name" },
                    txtName,
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
                        })
                }
            };
        }
    }
}