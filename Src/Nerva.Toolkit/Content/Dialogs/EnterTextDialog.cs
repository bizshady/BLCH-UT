using Eto.Drawing;
using Eto.Forms;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class EnterTextDialog : Dialog<DialogResult>
	{
        bool isShown = false;

        private string text;
        public string Text => text;

        TextBox tb = new TextBox();

        Button btnOk = new Button { Text = "OK" };
        Button btnCancel = new Button { Text = "Cancel" };

        public EnterTextDialog(string title, string text = null)
        {
            this.Title = title;
            ClientSize = new Size(400, 100);
            Topmost = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            CreateLayout();

            this.AbortButton = btnCancel;
            this.DefaultButton = btnOk;

            btnOk.Click += (s, e) =>
            {
                text = tb.Text;
                this.Close(DialogResult.Ok);
            };

            btnCancel.Click += (s, e) =>
            {
                text = null;
                this.Close(DialogResult.Cancel);
            };

            tb.Text = text;
        }

        public void CreateLayout()
        {
            Content = new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    tb,
                    new TableRow (
                        new TableLayout
                        {
                            Rows = {
                                new TableRow (
                                    new TableCell(null, true),
                                    btnOk,
                                    btnCancel)
                            }
                        }),
                    new TableRow { ScaleHeight = true }
                }
            };

            tb.Focus();
        }
    }
}