using Eto.Drawing;
using Eto.Forms;

namespace Nerva.Toolkit.Content.Dialogs
{
    public partial class EnterPasswordDialog : Dialog
	{
        public string Password { get; set; }

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

            pwr.Cells.Add(pwb);

            Content = new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    pwr,
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
        }
    }
}