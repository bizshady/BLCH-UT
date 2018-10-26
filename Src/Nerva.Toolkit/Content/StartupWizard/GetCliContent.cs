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

        Button btnDownload = new Button { Text = "Download", Enabled = false };
        ProgressBar pbDownload = new ProgressBar();

        public override Control CreateContent()
        {
            switch (OS.Type)
            {
                case OS_Type.Unsupported:
                    return CreateNotSupportedContent();
                default:
                    {
                        string link = null;
                        switch (OS.Type)
                        {
                            case OS_Type.Windows:
                                link = VersionManager.VersionInfo.WindowsLink;
                                break;
                            case OS_Type.Linux:
                                link = VersionManager.VersionInfo.LinuxLink;
                                break;
                            case OS_Type.Mac:
                                link = VersionManager.VersionInfo.MacLink;
                                break;
                        }

                        return CreateContent(link);
                    }
            }
        }

        private Control CreateContent(string link)
        {
            btnDownload.Click += (s, e) =>
            {
                HandleDownloadClick(link);
            };

            return new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new Label { Text = $"It appears you are running {OS.Type}" },
                    new Label { Text = "Click download to get the CLI tools" },
                    new Label { Text = "   " },
                    new StackLayoutItem(null, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new StackLayoutItem(btnDownload, false)
                        }
                    },
                    new Label { Text = "   " },
                    pbDownload
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
                    new Label { Text = "Only Linux, Windows & Mac are supported" }
                }
            };
        }

        public override void OnAssignContent()
        {
            btnDownload.Enabled = OS.Type != OS_Type.Unsupported;
            Parent.EnableNextButton(File.Exists(FileNames.GetCliExePath(FileNames.NERVAD)));
        }

        public override void OnNext()
        {
            Cli.Instance.StartDaemon();
            Cli.Instance.StartWallet();
        }

        private void HandleDownloadClick(string link)
        {
            btnDownload.Enabled = false;

            VersionManager.DownloadFile(link, (DownloadProgressChangedEventArgs ea) =>
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    btnDownload.Enabled = false;
                    pbDownload.MaxValue = (int)ea.TotalBytesToReceive;
                    pbDownload.Value = (int)ea.BytesReceived;
                });

            }, (bool success, string dest) =>
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    btnDownload.Enabled = true;

                    if (success)
                    {
                        Parent.EnableNextButton(true);
                        Configuration.Instance.ToolsPath = dest;
                        Log.Instance.Write("Setting Config.ToolsPath: {0}", dest);
                    }
                    else
                    {
                        if (File.Exists(dest))
                            File.Delete(dest);
                        MessageBox.Show(Application.Instance.MainForm, "An error occured while downloading/extracting the NERVA CLI tools.\r\n" +
                        "Please refer to the log file and try again later", "Request Failed", MessageBoxButtons.OK, MessageBoxType.Error, MessageBoxDefaultButton.OK);
                    }
                });
            });
        }
    }
}