using AngryWasp.Logger;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class AddressBookDialog : DialogBase<DialogResult>
	{
        public AddressBookDialog() : base("Address Book")
        {
            this.DefaultButton = btnCancel;
        }

        protected override Control ConstructChildContent()
        {
            return null;
        }

        protected override void OnOk()
        {
            this.Close(DialogResult.Ok);
        }

        protected override void OnCancel()
        {
            this.Close(DialogResult.Cancel);
        }
    }
}