using Eto.Forms;
using Nerva.Toolkit.CLI.Structures.Response;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class TransferDialog : Dialog<DialogResult>
	{
        Button btnOk = new Button { Text = "OK" };

        public TransferDialog(SubAddressAccount acc)
        {
        }
    }
}