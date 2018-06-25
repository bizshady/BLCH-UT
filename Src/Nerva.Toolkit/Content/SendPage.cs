using Eto.Forms;
using Nerva.Toolkit.CLI.Structures.Response;

namespace Nerva.Toolkit.Content
{
    public partial class SendPage
	{
		private Scrollable mainControl;
        public Scrollable MainControl => mainControl;

		public SendPage() { }

        public void ConstructLayout()
		{
			mainControl = new Scrollable();
		}

		public void Update(Account a)
		{
		}
    }
}