using System.Text.RegularExpressions;
using AngryWasp.Helpers;
using AngryWasp.Logger;
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
        Label lblImport = new Label { TextAlignment = TextAlignment.Right };
        ProgressBar pbImport = new ProgressBar();

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
                NewWalletDialog d2 = new NewWalletDialog();
                if (d2.ShowModal() == DialogResult.Ok)
                {
                    Helpers.TaskFactory.Instance.RunTask("createwallet", "Creating wallet", () =>
                    {
                        int result = Cli.Instance.Wallet.Interface.CreateNewWallet(d2.Name, d2.Password);

                        SaveWalletLogin(d2.Name, d2.Password);

                        if (result == 0)
                        {
                            Application.Instance.Invoke( () =>
                            {
                                lblImport.Text = "Wallet creation complete";
                                Parent.EnableNextButton(true);                                              
                            });
                        }
                        else
                        {
                            Application.Instance.Invoke( () =>
                            {
                                lblImport.Text = "Wallet creation failed";
                                Parent.EnableNextButton(true);     
                            });
                        }
                    });
                    
                }
                else
                    Parent.EnableNextButton(true); 
            };

            btnImportAccount.Click += (s, e) =>
            {
                Parent.EnableNextButton(false);
                ImportWalletDialog d2 = new ImportWalletDialog();
                DialogResult dr = d2.ShowModal();
                if (dr == DialogResult.Ok)
                {
                    Helpers.TaskFactory.Instance.RunTask("importwallet", "Importing wallet", () =>
                    {
                        Log.Instance.AddWriter("wizard", new LabelWriter(lblImport, pbImport), false);

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
                            
                        Log.Instance.RemoveWriter("wizard");
                            
                        SaveWalletLogin(d2.Name, d2.Password);

                        if (result == 0)
                        {
                            Application.Instance.Invoke( () =>
                            {
                                lblImport.Text = "Wallet import complete";
                                Parent.EnableNextButton(true);                                              
                            });
                        }
                        else
                        {
                            Application.Instance.Invoke( () =>
                            {
                                lblImport.Text = "Wallet import failed";
                                Parent.EnableNextButton(true);     
                            });
                        }
                    });
                }
                else
                    Parent.EnableNextButton(true); 
                
            };

            return new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items = 
                {
                    new Label { Text = "Create your account" },
                    new Label { Text = "   " },
                    new Label { Text = "You must create an account in order" },
                    new Label { Text = "to mine, send or receive NERVA" },
                    new Label { Text = "You can create a new account, or" },
                    new Label { Text = "import an existing one" },
                    new Label { Text = "   " },
                    new StackLayoutItem(null, true),
                    new StackLayoutItem(new StackLayout
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
                    }, false),
                    pbImport,
                    lblImport
                }
            };
        }

        public static void SaveWalletLogin(string walletFile, string password)
        {
            string formattedPassword = string.IsNullOrEmpty(password) ? string.Empty : password.EncodeBase64();
            Configuration.Instance.Wallet.LastOpenedWallet = walletFile;
            Configuration.Instance.Wallet.LastWalletPassword = formattedPassword;
        }

        public override void OnAssignContent()
        {
            Parent.EnableNextButton(true);
        }
    }

    public class LabelWriter : ILogWriter
    {
        private Label lbl;
        private ProgressBar pb;

        public LabelWriter(Label lbl, ProgressBar pb)
        {
            this.lbl = lbl;
            this.pb = pb;
        }

        public void Close()
        {
            return;
        }

        public void Flush()
        {
            return;
        }

        public void Write(Log_Severity severity, string value)
        {
            Application.Instance.AsyncInvoke( () =>
            {
                string stripped = Regex.Match(value, @"\d+(\.\d+)?[ ]\/[ ]\d+(\.\d+)?").Value;
                lbl.Text = stripped;
                if (stripped != null)
                {
                    string[] split = stripped.Split('/');
                    if (split.Length != 2)
                        return;

                    int val = int.Parse(split[0]);
                    int max = int.Parse(split[1]);

                    pb.MaxValue = max;
                    pb.Value = val;
                }
            });
        }
    }
}