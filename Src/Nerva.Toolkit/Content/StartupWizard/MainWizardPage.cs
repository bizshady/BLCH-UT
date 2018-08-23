using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Wizard
{
    public class MainWizardDialog : WizardDialogBase<DialogResult>
    {
        public MainWizardDialog(string title) : base (title)
        {
           
        }

        protected override Control ConstructChildContent()
        {
            return new StackLayout
            {
                Orientation = Orientation.Horizontal,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = 10,
                Spacing = 10,
                Items = 
                {
                    new StackLayoutItem(new ImageView
                    {
                        Image = Bitmap.FromResource("Nerva.Toolkit.NERVA-Logo.png", Assembly.GetExecutingAssembly())
                    }, false),
                    new StackLayoutItem(new StackLayout
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Padding = new Padding(0, 0, 0, 10),
                        Spacing = 0,
                        Items = 
                        {
                            new Label { Text = "Welcome to NERVA" },
                            new Label { Text = "   " },
                            new Label { Text = "This wizard will guide you through all" },
                            new Label { Text = "the steps required to get up and running" },
                            new Label { Text = "Click >> to continue" },
                        }
                    }, true),
                }
            };
        }

        protected override void OnBack()
        {
            
        }

        protected override void OnNext()
        {
            
        }
    }
}