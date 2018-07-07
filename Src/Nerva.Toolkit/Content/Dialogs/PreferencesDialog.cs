using System;
using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class PreferencesDialog : DialogBase<DialogResult>
    {
        private CheckBox chkStopOnExit = new CheckBox();
        private CheckBox chkAutoStartMining = new CheckBox();
        private TextBox txtMiningAddress = new TextBox();
        private ComboBox cbxMiningThreads = new ComboBox();

        public PreferencesDialog() : base("Preferences")
        {
        }

        protected override Control ConstructChildContent()
        {
            chkStopOnExit.Checked = Configuration.Instance.Daemon.StopOnExit;
            chkAutoStartMining.Checked = Configuration.Instance.Daemon.AutoStartMining;
            txtMiningAddress.Text = Configuration.Instance.Daemon.MiningAddress;

            for (int i = 0; i < Environment.ProcessorCount; i++)
                cbxMiningThreads.Items.Add(i.ToString());

            return new TabControl
            {
                Pages = 
                {
                    new TabPage
                    {
                        Text = "General",
                        Content = new StackLayout
                        {
                            Padding = 10,
                            Spacing = 10,
                            Orientation = Orientation.Vertical,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Items = 
                            {
                                
                            }
                        }
                    },
                    new TabPage
                    {
                        Text = "Daemon",
                        Content = 
                        {

                        }
                    },
                    new TabPage
                    {
                        Text = "Wallet",
                        Content = 
                        {

                        }
                    }
                }
            };
        }

        protected override void OnCancel()
        {
            this.Close(DialogResult.Cancel);
        }

        protected override void OnOk()
        {
            this.Close(DialogResult.Ok);
        }
    }
}