using AngryWasp.Logger;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class DisplaySeedDialog : Dialog
	{
        Button btnSave = new Button { Text = "Save" };

        Button btnClose = new Button { Text = "Close" };

        TextArea txtKey = new TextArea { ReadOnly = true };

        public DisplaySeedDialog(Key_Type kt)
        {
            this.Title = (kt == Key_Type.View_Key ? "View Key" : "Mnemonic Seed");
            ClientSize = new Size(400, 300);
            Topmost = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            string key = Cli.Instance.Wallet.QueryKey(kt);
            txtKey.Text = key;
            txtKey.Wrap = true;

            CreateLayout(kt);

            this.AbortButton = btnClose;
            this.DefaultButton = btnClose;

            btnClose.Click += (s, e) => { this.Close(); };

            btnSave.Click += (s, e) =>
            {
                Log.Instance.Write("Saving keys not implemented");
                this.Close();
            };
        }

        public void CreateLayout(Key_Type kt)
        {
            Content = new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    new TableRow(
                        txtKey)
                        {
                            ScaleHeight = true
                        },
                    new TableRow (
                        new TableLayout
                        {
                            Rows = {
                                new TableRow (
                                    new TableCell(null, true),
                                    btnSave,
                                    btnClose)
                            }
                        })
                }
            };
        }
    }
}