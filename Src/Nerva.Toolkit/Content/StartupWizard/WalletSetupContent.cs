using AngryWasp.Helpers;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Content.Dialogs;

namespace Nerva.Toolkit.Content.Wizard
{
    public class WalletSetupContent : WizardContent
    {
        private Control content;

        public override string Title => "Set up your account";

        Button btnCreateAccount = new Button { Text = "Create" };
        Button btnImportAccount = new Button { Text = "Import" };

        public override Control Content
        {
            get
            {
                if (content == null)
                    content = CreateContent();

                return content;
            }
        }

        public override Control CreateContent()
        {
            btnCreateAccount.Click += (s, e) =>
            {
                Parent.EnableNextButton(false);
                while (true)
                {
                    NewWalletDialog d2 = new NewWalletDialog();
                    if (d2.ShowModal() == DialogResult.Ok)
                    {
                        bool created = Cli.Instance.Wallet.Interface.CreateWallet(d2.Name, d2.Password);

                        if (created)
                        {
                            SaveWalletLogin(d2.Name, d2.Password);
                            Parent.EnableNextButton(true);
                            return;
                        }
                    }
                    else
                        return;
                }
            };

            btnImportAccount.Click += (s, e) =>
            {
                Parent.EnableNextButton(false);
                while (true)
                {
                    ImportWalletDialog d2 = new ImportWalletDialog();
                    DialogResult dr = d2.ShowModal();
                    if (dr == DialogResult.Ok)
                    {
                        Helpers.TaskFactory.Instance.RunTask("importwallet", "Importing wallet", () =>
                        {
                            int result = -1;
                            switch (d2.ImportType)
                            {
                                case Import_Type.Key:
                                    result = Cli.Instance.Wallet.Interface.RestoreWalletFromKeys(d2.Name, d2.ViewKey, d2.SpendKey, d2.Password, d2.Language);
                                break;
                                case Import_Type.Seed:
                                    result = Cli.Instance.Wallet.Interface.RestoreWalletFromSeed(d2.Name, d2.Seed, d2.Password, d2.Language);
                                break;
                            } 

                            SaveWalletLogin(d2.Name, d2.Password);

                            if (result == 0)
                            {
                                Application.Instance.AsyncInvoke( () =>
                                {
                                    MessageBox.Show(Application.Instance.MainForm, "Wallet import complete", "Wallet Import",
                                        MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);

                                    Parent.EnableNextButton(true);
                                });
                            }
                            else
                            {
                                Application.Instance.AsyncInvoke( () =>
                                {
                                    MessageBox.Show(Application.Instance.MainForm, "Wallet import failed.\r\nCheck the log file for errors", "Wallet Import",
                                        MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
                                });
                            }
                        });
                                    
                        return;
                    }
                    else
                        return;
                }
            };

            return new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = new Padding(0, 0, 0, 10),
                Spacing = 0,
                Items = 
                {
                    new Label { Text = "Create your account" },
                    new Label { Text = "   " },
                    new Label { Text = "You must create an account in order" },
                    new Label { Text = "to mine, send or receive NERVA" },
                    new Label { Text = "You can create a new account, or" },
                    new Label { Text = "import an existing one" },
                    new Label { Text = "    " },
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Padding = new Padding(0, 0, 0, 10),
                        Spacing = 10,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new StackLayoutItem(btnCreateAccount, false),
                            new StackLayoutItem(btnImportAccount, false)
                        }
                    }
                }
            };
        }

        public static void SaveWalletLogin(string walletFile, string password)
        {
            string formattedPassword = string.IsNullOrEmpty(password) ? string.Empty : StringHelper.EncodeBase64(password);
            Configuration.Instance.Wallet.LastOpenedWallet = walletFile;
            Configuration.Instance.Wallet.LastWalletPassword = formattedPassword;
        }
    }
}