using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Wizard
{
    public class GetCliDialog : WizardDialogBase
    {
        protected override Control ConstructChildContent()
        {
            ComboBox cbxDistro = new ComboBox();
            cbxDistro.DataStore = Enum.GetNames(typeof(Supported_Systems));

            ComboBox cbxDistroVersion = new ComboBox();
            cbxDistro.SelectedIndexChanged += (s, e) =>
            {
                Dictionary<string, string> distros = null;

                switch ((Supported_Systems)cbxDistro.SelectedIndex)
                {
                    case Supported_Systems.Debian:
                        distros = VersionManager.VersionInfo.DebianLinks;
                        break;
                    case Supported_Systems.Ubuntu:
                        distros = VersionManager.VersionInfo.UbuntuLinks;
                        break;
                    case Supported_Systems.Fedora:
                        distros = VersionManager.VersionInfo.FedoraLinks;
                        break;
                }

                cbxDistroVersion.ItemTextBinding = Binding.Property((KeyValuePair<string, string> r) => r.Key);
                cbxDistroVersion.ItemKeyBinding = Binding.Property((KeyValuePair<string, string> r) => r.Value);
                cbxDistroVersion.DataStore = distros.Cast<object>();
            };
            
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
                            new Label { Text = "Select your Backend" },
                            new Label { Text = "   " },
                            new Label { Text = "The NERVA GUI needs the Command line tools to operate" },
                            new Label { Text = "Please select your OS from the list" },
                            cbxDistro,
                            cbxDistroVersion
                        }
                    }, true),
                }
            };
        }
    }
}