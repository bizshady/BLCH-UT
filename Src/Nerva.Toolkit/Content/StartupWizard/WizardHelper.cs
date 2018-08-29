using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using AngryWasp.Helpers;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Wizard
{
    public abstract class WizardContent
    {
        public abstract string Title { get; }
        public abstract Control Content { get; }
        public abstract Control CreateContent();
    }

    public class SetupWizard
    {
        WizardDialog dlg;

        public SetupWizard()
        {
            WizardContent[] pages = new WizardContent[]
            {
                new IntroContent(),
                new GetCliContent()
            };

            dlg = new WizardDialog(pages);

            dlg.AllowNavigation(false);

            if (VersionManager.VersionInfo == null)
                VersionManager.GetVersionInfo(() =>
                {
                    if (VersionManager.VersionInfo == null)
                    {
                        //Todo: messagebox could not retrieve cli tools
                        Application.Instance.AsyncInvoke( () =>
                        {
                            if (MessageBox.Show(Application.Instance.MainForm, "Version information could not be retrieved at this time.\r\nCannot find download links to the CLI tools.\r\n" +
                                "Would you like to continue anyway?\r\nYou will need to provide your own NERVA CLI tools", "Request Failed", 
                                MessageBoxButtons.YesNo, MessageBoxType.Question, MessageBoxDefaultButton.Yes) == DialogResult.No)
                                Environment.Exit(0);
                            
                            dlg.AllowNavigation(true);
                        });
                    }
                    else
                        dlg.AllowNavigation(true);
                });
        }

        public void Run()
        {
            dlg.ShowModal();
        }
    }
}