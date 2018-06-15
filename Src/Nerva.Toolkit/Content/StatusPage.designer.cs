using Eto.Forms;

namespace Nerva.Toolkit.Content
{
    public partial class StatusPage : ITabPageContent
	{
		private Scrollable mainControl;

		public Control MainControl => mainControl;

		public StatusPage()
		{
			mainControl = new Scrollable
			{ 
				Content = new TableLayout
				{
					Padding = 10,
					Spacing = new Eto.Drawing.Size(10, 10),
					Rows =
					{
						new TableRow (
								new TableCell(new Label { Text = "Daemon Status" })),
						new TableRow(
								new TableCell(new Label { Text = "Height:" }),
								new TableCell(new Label { Text = "0" })),
						new TableRow(
								new TableCell(new Label { Text = "Connections" })),
						new TableRow(
							new TableCell(new Label { Text = "Address" }),
							new TableCell(new Label { Text = "Height" }),
							new TableCell(new Label { Text = "Status" }),
							new TableCell(null, true),
							new TableCell(null))
					}
				}
			};

			//Simulate 100 connection entries to test scrollable content
			TableLayout tl = mainControl.Content as TableLayout;

			for (int i = 0; i < 100; i++)
			{
				tl.Rows.Add(
					new TableRow(
						new TableCell(new Label { Text = $"127.0.0.{i}:{3524 + i * 2}" }),
						new TableCell(new Label { Text = "100000" }),
						new TableCell(new Label { Text = "state_normal" }),
						new TableCell(null, true),
						new TableCell(new Button { Text = "Ban" }))
				);
			}
		}
    }
}