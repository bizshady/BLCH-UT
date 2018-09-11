using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using AngryWasp.Logger;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Config;
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
        Button btnDownload = new Button { Text = "Download", Enabled = false };
        ProgressBar pbDownload = new ProgressBar { Visible = false, };

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
            return new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = new Padding(0, 0, 0, 10),
                Spacing = 0,
                Items = 
                {
                    new Label { Text = "It appears you are running Windows" },
                    new Label { Text = "Click download to get the CLI tools" },
                    new Label { Text = "   " },
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Items =
                        {
                            new StackLayoutItem(pbDownload, true),
                            new StackLayoutItem(btnDownload, false)
                        }
                    }
                }
            };
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

            cbxDistro.SelectedIndex = 0;

            btnDownload.Click += (s, e) =>
            {
                btnDownload.Enabled = false;
                
                string file = cbxDistroVersion.SelectedKey;
                VersionManager.DownloadFile(file, (DownloadProgressChangedEventArgs ea) =>
                {
                    Application.Instance.AsyncInvoke(() =>
                    {
                        pbDownload.Visible = true;
                        btnDownload.Enabled = false;
                        pbDownload.MaxValue = (int)ea.TotalBytesToReceive;
                        pbDownload.Value = (int)ea.BytesReceived;
                    });
                    
                }, (bool success, string destDir) =>
                {
                    btnDownload.Enabled = true;

                    if (success)
                    {
                        Parent.EnableNextButton(true);
                        pbDownload.Visible = false;
                        Configuration.Instance.ToolsPath = destDir;
                        Log.Instance.Write("Setting Config.ToolsPath: {0}", destDir);
                    }
                    else
                        MessageBox.Show(Application.Instance.MainForm, "An error occured while downloading/extracting the NERVA CLI tools.\r\n" + 
                        "Please refer to the log file and try again later", "Request Failed", MessageBoxButtons.OK, MessageBoxType.Error, MessageBoxDefaultButton.OK);
                });
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
                    new Label { Text = "The NERVA GUI needs the CLI tools" },
                    new Label { Text = "to operate." },
                    new Label { Text = "Please select your OS from the list" },
                    new Label { Text = "to download the latest version" },
                    new Label { Text = "   " },
                    new TableLayout
					{
						Padding = new Padding(0, 0, 0, 10),
						Spacing = new Eto.Drawing.Size(10, 10),
						Rows =
						{
							new TableRow(
								new TableCell(new Label { Text = "Distro:" }),
								new TableCell(cbxDistro, true)),
							new TableRow(
								new TableCell(new Label { Text = "Version:" }),
								new TableCell(cbxDistroVersion, true))
						}
					},
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Padding = new Padding(0, 0, 0, 10),
                        Spacing = 10,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new StackLayoutItem(btnDownload, false)
                        }
                    },
                    pbDownload
                }
            };
        }

        public override void OnAssignContent()
        {
            pbDownload.Visible = false;
            btnDownload.Enabled = cbxDistro.SelectedIndex != -1 && cbxDistroVersion.SelectedIndex != -1;
            
            if (File.Exists(FileNames.GetCliExePath(FileNames.NERVAD)))
            {
                Parent.EnableNextButton(true);
                Parent.Title = Title + " - Complete!";
            }
            else
            {
                Parent.EnableNextButton(false);
                Parent.Title = Title + " - Incomplete!";
            }
        }

        public override void OnNext()
        {
            Cli.Instance.StartDaemon();
            Cli.Instance.StartWallet();
        }
    }
}