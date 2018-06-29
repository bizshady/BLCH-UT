using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Request;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class TransferDialog : DialogBase<DialogResult>
	{
        private Send txData = null;
        private SubAddressAccount accData;

        public Send TxData => txData;

        TextBox txtAddress = new TextBox();
        TextBox txtPaymentId = new TextBox();
        TextBox txtAmount = new TextBox();
        EnumDropDown<Send_Priority> cbxPriority = new EnumDropDown<Send_Priority>();
        Label lblAccount = new Label();
        Label lblAmount = new Label();

        public TransferDialog(SubAddressAccount accData) : base("Transfer NERVA")
        {
            this.accData = accData;
            lblAccount.Text = $"{Conversions.WalletAddressShortForm(accData.BaseAddress)} ({(string.IsNullOrEmpty(accData.Label) ? "No Label" : accData.Label)})";
            lblAmount.Text = Conversions.FromAtomicUnits(accData.Balance).ToString();
            cbxPriority.SelectedIndex = 0;
        }

        protected override void OnOk()
        {
            if (MessageBox.Show(this, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxType.Question, MessageBoxDefaultButton.Yes) == DialogResult.Yes)
            {
                double amt = 0;
                if (!double.TryParse(txtAmount.Text, out amt))
                {
                    MessageBox.Show(this, "Amount to send is incorrect format", MessageBoxType.Error);
                    return;
                }

                txData = Cli.Instance.Wallet.TransferFunds(accData, txtAddress.Text, txtPaymentId.Text, amt, cbxPriority.SelectedValue);
                this.Close(DialogResult.Ok);
            }
        }

        protected override void OnCancel()
        {
            txData = null;
            this.Close(DialogResult.Cancel);
        }

        protected override Control ConstructChildContent()
        {
            return new TableLayout
            {
                Padding = 10,
				Spacing = new Eto.Drawing.Size(10, 10),
                Rows = {
                    new Label { Text = "Send From"},
                    lblAccount,
                    new Label { Text = "Balance" },
                    lblAmount,
                    new Label { Text = "Send To" },
                    txtAddress,
                    new Label { Text = "Payment ID" },
                    txtPaymentId,
                    new TableLayout
                    {
				        Spacing = new Eto.Drawing.Size(10, 10),
                        Rows = {
                            new TableRow(new Label { Text = "Amount" }, new Label { Text = "Priority"} ),
                            new TableRow(txtAmount, cbxPriority)
                        }
                    },
                    new TableRow { ScaleHeight = true }
                }
            };
        }
    }
}