using System.ComponentModel;
using System.Text;
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
            if (IsImportStarted())
                return;

            base.OnOk();

            BackgroundWorker w = new BackgroundWorker();

            int index = tc.SelectedIndex;
            string lang = cbxLang.SelectedValue.ToString();

            string name = txtName.Text;
            string viewKey = txtViewKey.Text;
            string spendKey = txtSpendKey.Text;
            string seed = txtSeed.Text;

            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrEmpty(name))
                errors.AppendLine("Wallet name not provided");

            switch (index)
            {
                case 0:
                {
                    if (string.IsNullOrEmpty(viewKey))
                        errors.AppendLine("View key not provided");

                    if (string.IsNullOrEmpty(spendKey))
                        errors.AppendLine("Spend key not provided");
                }
                break;
                case 1:
                {
                    if (string.IsNullOrEmpty(seed))
                        errors.AppendLine("Seed not provided");
                }
                break;
            }
            
            string errorString = errors.ToString();
            if (!string.IsNullOrEmpty(errorString))
            {
                MessageBox.Show(this, $"Please remedy the following errors:\r\n{errorString}", MessageBoxType.Error);
                return;
            }

            w.DoWork += (s, e) =>
            {
                importStarted = true;
                int result = -1;
                switch (index)
                {
                    case 0:
                        result = Cli.Instance.Wallet.Interface.RestoreWalletFromKeys(name, viewKey, spendKey, password, lang);
                    break;
                    case 1:
                        result = Cli.Instance.Wallet.Interface.RestoreWalletFromSeed(name, seed, password, lang);
                    break;
                } 

                e.Result = result;
            };

            w.RunWorkerCompleted += (s, e) =>
            {
                int result = (int)e.Result;
                if (result == 0)
                {
                    MessageBox.Show(this, "Wallet import complete", "Wallet Import",
                        MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);

                    this.Close(DialogResult.Ok);
                }
                else
                {
                    MessageBox.Show(this, "Wallet import failed.\r\nCheck the log file for errors", "Wallet Import",
                        MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);

                    this.Close(DialogResult.Abort);
                }
            };

            w.RunWorkerAsync();
        }

        protected override void OnCancel()
        {
            if (IsImportStarted())
                return;

            base.OnCancel();
            name = null;
            this.Close(DialogResult.Cancel);
        }

        private bool IsImportStarted()
        {
            if (importStarted)
            {
                MessageBox.Show(this, "Please wait for the import to complete", "Wallet Import",
                    MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
                return true;
            }

            return false;
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