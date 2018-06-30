using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class ImportWalletDialog : PasswordDialog
    {
        private string name;
        public string Name => name;

        TextBox txtName = new TextBox();
        TextBox txtViewKey = new TextBox();
        TextBox txtSpendKey = new TextBox();

        public ImportWalletDialog() : base("Import Wallet") { }

        bool importStarted = false;
        protected override void OnOk()
        {
            if (importStarted)
            {
                MessageBox.Show("Please wait for the import to complete");
                return;
            }

            base.OnOk();

            BackgroundWorker w = new BackgroundWorker();

            w.DoWork += (ws, we) =>
            {
                importStarted = true;
                Application.Instance.AsyncInvoke ( () =>
				{
                    Content.Enabled = false;
				});

                Cli.Instance.Wallet.RestoreWalletFromKeys(name, txtViewKey.Text, txtSpendKey.Text, password);
            };

            w.RunWorkerCompleted += (ws, we) => 
            {
                MessageBox.Show("wallet import complete");
                this.Close(DialogResult.Ok);
            };

            w.RunWorkerAsync();
        }

        protected override void OnCancel()
        {
            if (importStarted)
            {
                MessageBox.Show("Please wait for the import to complete");
                return;
            }

            base.OnCancel();
            name = null;
            this.Close(DialogResult.Cancel);
        }

        protected override Control ConstructChildContent()
        {
            return new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
                Items = 
                {
                    new StackLayoutItem(new Label { Text = "Wallet Name" }),
                    new StackLayoutItem(txtName),
                    new StackLayoutItem(new Label { Text = "View Key" }),
                    new StackLayoutItem(txtViewKey),
                    new StackLayoutItem(new Label { Text = "Spend Key" }),
                    new StackLayoutItem(txtSpendKey),
                    new StackLayoutItem(new Label { Text = "Password" }),
                    ConstructPasswordControls()
                }
            };
        }
    }
}