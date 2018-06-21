using Eto.Drawing;
using Eto.Forms;

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

        TextBox txtName = new TextBox();

        TableRow pwr = new TableRow();

        Button btnShow = new Button { Text = "Show" };
        Button btnOk = new Button { Text = "OK" };
        Button btnCancel = new Button { Text = "Cancel" };

        public OpenWalletDialog()
        {
            this.Title = "Create New Wallet";
            ClientSize = new Size(400, 100);
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
                        }),
                    new TableRow { ScaleHeight = true }
                }
            };
        }
    }
}