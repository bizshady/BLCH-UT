using Eto.Drawing;
using Eto.Forms;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class NewWalletDialog : PasswordDialog
	{
        protected string name;
        public string Name => name;

        protected TextBox txtName = new TextBox();

        public NewWalletDialog(string title = "Create New Wallet") : base(title)
        {
            //the RPC wallet needs to be open to create a new wallet
            CLI.Cli.Instance.Wallet.ResumeCrashCheck();
        }

        protected override void OnOk()
        {
            base.OnOk();
            name = txtName.Text;
            this.Close(DialogResult.Ok);
        }

        protected override void OnCancel()
        {
            base.OnCancel();
            name = null;
            this.Close(DialogResult.Cancel);
        }

        protected override void OnShow()
        {
            string oldName = txtName.Text;
            base.OnShow();
            txtName.Text = oldName;
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
                    new StackLayoutItem(new Label { Text = "Password" }),
                    ConstructPasswordControls()
                }
            };
        }
    }
}