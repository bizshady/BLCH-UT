using System;
using AngryWasp.Helpers;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Request;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Helpers;
using static Nerva.Toolkit.CLI.WalletInterface;

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
        ComboBox cbxPriority = new ComboBox();
        Label lblAccount = new Label();
        Label lblAmount = new Label();

        Button btnGenPayId = new Button { Text = "Generate" };

        public TransferDialog(SubAddressAccount accData) : base("Transfer NERVA")
        {
            this.accData = accData;
            lblAccount.Text = $"{Conversions.WalletAddressShortForm(accData.BaseAddress)} ({(string.IsNullOrEmpty(accData.Label) ? "No Label" : accData.Label)})";
            lblAmount.Text = Conversions.FromAtomicUnits(accData.Balance).ToString();
            cbxPriority.DataStore = Enum.GetNames(typeof(Send_Priority));
            cbxPriority.SelectedIndex = 0;

            btnGenPayId.Click += (s, e) => txtPaymentId.Text = Conversions.GenerateRandomPaymentID();
            
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

                //todo: need to validate address that it is correct format
                if (string.IsNullOrEmpty(txtAddress.Text))
                {
                    MessageBox.Show(this, "Address not provided", MessageBoxType.Error);
                    return;
                }

                if (txtPaymentId.Text.Length != 0 && (txtPaymentId.Text.Length != 16 && txtPaymentId.Text.Length != 64))
                {
                    MessageBox.Show(this, "Payment ID must be 16 or 64 characters long\r\nCurrent Payment ID length is " + 
                        txtPaymentId.Text.Length + " characters", MessageBoxType.Error);
                    return;
                }  

                RpcWalletError e = new RpcWalletError();

                txData = Cli.Instance.Wallet.TransferFunds(accData, txtAddress.Text, txtPaymentId.Text, amt, (Send_Priority)cbxPriority.SelectedIndex, ref e);

                if (e.Code != 0)
                {
                    MessageBox.Show(this, $"The transfer request returned RPC error:\r\n{e.Message}", MessageBoxType.Error);
                    return;
                }

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
            return new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new Label { Text = "Send From"},
                    lblAccount,
                    new Label { Text = "Balance" },
                    lblAmount,
                    new Label { Text = "Send To" },
                    txtAddress,
                    new TableLayout
                    {
				        Spacing = new Eto.Drawing.Size(10, 10),
                        Rows = {
                            new TableRow(new TableCell(new Label { Text = "Payment ID" }, true) ),
                            new TableRow(txtPaymentId, btnGenPayId),
                            new TableRow(new Label { Text = "Amount" }, new Label { Text = "Priority"} ),
                            new TableRow(txtAmount, cbxPriority)
                        }
                    }
                }
            };
        }
    }
}