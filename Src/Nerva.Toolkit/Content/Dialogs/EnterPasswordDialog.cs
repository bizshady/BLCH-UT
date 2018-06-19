using Eto.Drawing;
using Eto.Forms;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class EnterPasswordDialog : Dialog<DialogResult>
	{
        bool isShown = false;

        private string password;
        public string Password => password;

        PasswordBox pwb = new PasswordBox { PasswordChar = '*' };
        TextBox tb = new TextBox();
        TableRow pwr = new TableRow();

        Button btnShow = new Button { Text = "Show" };
        Button btnOk = new Button { Text = "OK" };
        Button btnClose = new Button { Text = "Close" };

        public EnterPasswordDialog()
        {
            this.Title = "Enter Password";
            ClientSize = new Size(400, 100);

            CreateLayout();

            this.AbortButton = btnClose;
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

                this.Close(DialogResult.Ok);
            };

            btnClose.Click += (s, e) =>
            {
                password = null;
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
                    textControl,
                    new TableRow (
                        new TableLayout
                        {
                            Rows = {
                                new TableRow (
                                    btnShow,
                                    new TableCell(null, true),
                                    btnOk,
                                    btnClose)
                            }
                        }),
                    new TableRow { ScaleHeight = true }
                }
            };

            textControl.Focus();
        }
    }
}