using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class DisplaySeedDialog : DialogBase<DialogResult>
	{
        private Key_Type keyType;
        TextArea txtKey = new TextArea { ReadOnly = true, Wrap = true };

        public DisplaySeedDialog(Key_Type kt) : base(kt == Key_Type.View_Key ? "View Key" : "Mnemonic Seed")
        {
            this.keyType = kt;
            txtKey.Text = Cli.Instance.Wallet.QueryKey(kt);

            //reuse ok and cancle buttonsa but give a more meaningful label
            btnOk.Text = "Save";
            btnCancel.Text = "Close";

            this.DefaultButton = btnCancel;
        }

        protected override Control ConstructChildContent()
        {
            return new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    new TableRow(txtKey) { ScaleHeight = true }}
            };
        }

        protected override void OnOk()
        {
            Log.Instance.Write("Saving keys not implemented");
            this.Close(DialogResult.Ok);
        }

        protected override void OnCancel()
        {
            this.Close(DialogResult.Cancel);
        }
    }
}