using Eto.Drawing;
using Eto.Forms;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class EnterPasswordDialog : DialogBase<DialogResult>
	{
        bool isShown = false;
        private string password;
        public string Password => password;

        PasswordBox pwb = new PasswordBox { PasswordChar = '*' };
        TextBox tb = new TextBox();
        TableRow pwr = new TableRow();
        private TextControl txtCtrl;

        Button btnShow = new Button { Text = "Show" };

        public EnterPasswordDialog() : base("Enter Wallet Password")
        {
            btnShow.Click += (s, e) => OnShow();
        }

        private void OnShow()
        {
            isShown = !isShown;
            if (isShown)
                tb.Text = pwb.Text;
            else
                pwb.Text = tb.Text;

            ConstructContent();
        }

        protected override void OnOk()
        {
            password = isShown ? tb.Text : pwb.Text;
            this.Close(DialogResult.Ok);
        }

        protected override void OnCancel()
        {
            password = null;
            this.Close(DialogResult.Cancel);
        }

        protected override void ConstructContent()
        {
            txtCtrl = isShown ? (TextControl)tb : (TextControl)pwb;
            base.ConstructContent();
            txtCtrl.Focus();
        }

        protected override Control ConstructChildContent()
        {
            return new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    new TableRow(txtCtrl, btnShow),
                    new TableRow { ScaleHeight = true }}
            };
        }
    }
}