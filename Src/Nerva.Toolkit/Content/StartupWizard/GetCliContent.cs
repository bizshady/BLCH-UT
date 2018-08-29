using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Wizard
{
    public class GetCliContent : WizardContent
    {
        private Control content;

        public override string Title => "Download NERVA";

        public override Control Content
        {
            get
            {
                if (content == null)
                    content = CreateContent();

                return content;
            }
        }

        ComboBox cbxDistro;
        ComboBox cbxDistroVersion;
        Button btnDownload = new Button { Text = "Download" };

        public override Control CreateContent()
        {
            switch (OS.Type)
            {
                case OS_Type.Windows:
                   return CreateWindowsChildContent();
                case OS_Type.Linux:
                    return CreateLinuxChildContent();
                default:
                    return CreateNotSupportedContent();
            }
        }

        private Control CreateWindowsChildContent()
        {
            return null;
        }

        private Control CreateNotSupportedContent()
        {
            return new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = new Padding(0, 0, 0, 10),
                Spacing = 0,
                Items = 
                {
                    new Label { Text = "This platform is not supported" },
                    new Label { Text = "   " },
                    new Label { Text = "Only Linux and Windows are supported" }
                }
            };
        }

        private Control CreateLinuxChildContent()
        {
            cbxDistro = new ComboBox();
            cbxDistroVersion = new ComboBox();
            cbxDistro.DataStore = Enum.GetNames(typeof(Supported_Systems));
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

                cbxDistroVersion.SelectedIndex = 0;
            };
            
            return new StackLayout
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
                    new Label { Text = "The NERVA GUI needs the CLI tools." },
                    new Label { Text = "Please select your OS from the list" },
                    new Label { Text = "to download the latest version" },
                    new Label { Text = "   " },
                    new TableLayout
					{
						Padding = 10,
						Spacing = new Eto.Drawing.Size(10, 10),
						Rows =
						{
							new TableRow(
								new TableCell(new Label { Text = "Distro:" }),
								new TableCell(cbxDistro, true),
								new TableCell(null)),
							new TableRow(
								new TableCell(new Label { Text = "Version:" }),
								new TableCell(cbxDistroVersion, true),
								new TableCell(null))
						}
					}
                    

                }
            };
        }

        
    }
}