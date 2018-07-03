using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class ImportWalletDialog : PasswordDialog
    {
        private string name;
        public string Name => name;

        TextBox txtName = new TextBox();
        TextBox txtViewKey = new TextBox();
        TextBox txtSpendKey = new TextBox();

        TextArea txtSeed = new TextArea();

        TabControl tc = new TabControl();
        ComboBox cbxLang = new ComboBox();

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

            int index = tc.SelectedIndex;
            string lang = cbxLang.SelectedValue.ToString();

            string name = txtName.Text;
            string viewKey = txtViewKey.Text;
            string spendKey = txtSpendKey.Text;
            string seed = txtSeed.Text;

            w.DoWork += (ws, we) =>
            {
                importStarted = true;
                switch (index)
                {
                    case 0:
                        Cli.Instance.Wallet.RestoreWalletFromKeys(name, viewKey, spendKey, password, lang);
                    break;
                    case 1:
                        Cli.Instance.Wallet.RestoreWalletFromSeed(name, seed, password, lang);
                    break;
                } 
            };

            w.RunWorkerCompleted += (ws, we) =>
            {
                MessageBox.Show(this, "Wallet import complete", "Wallet Import", MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
                name = txtName.Text;
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
            cbxLang.Items.Clear();

            foreach (var l in Constants.Languages)
                cbxLang.Items.Add(l);

            cbxLang.SelectedIndex = 1; //default to english

            tc = new TabControl
            {
                Pages =
                {
                    new TabPage
                    {
                        Text = "Keys",
                        Content = new StackLayout
                        {
                            Padding = 10,
                            Spacing = 10,
                            Orientation = Orientation.Vertical,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Items =
                            {
                                new StackLayoutItem(new Label { Text = "View Key" }),
                                new StackLayoutItem(txtViewKey, true),
                                new StackLayoutItem(new Label { Text = "Spend Key" }),
                                new StackLayoutItem(txtSpendKey, true)
                            }
                        }
                    },
                    new TabPage
                    {
                        Text = "Seed",
                        Content = new StackLayout
                        {
                            Padding = 10,
                            Spacing = 10,
                            Orientation = Orientation.Vertical,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Items =
                            {
                                new StackLayoutItem(new Label { Text = "Seed" }),
                                new StackLayoutItem(txtSeed, true),
                            }
                        }
                    }
                }
            };

            return new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(tc),
                    new StackLayoutItem(new Label { Text = "Wallet Name" }),
                    new StackLayoutItem(txtName),
                    new StackLayoutItem(new Label { Text = "Language" }),
                    new StackLayoutItem(cbxLang),
                    new StackLayoutItem(new Label { Text = "Password" }),
                    ConstructPasswordControls()
                }
            };
        }
    }
}