using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using AngryWasp.Helpers;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Wizard
{
    public enum Wizard_Dialog
    {
        Any,
        Intro,
        CliDownloader
    }

    public enum Wizard_Dialog_Result
    {
        None,
        Abort,
        Next,
        Back,
        Cancel
    }

    public class WizardDialogBase : Dialog<Wizard_Dialog_Result>
    {
        public delegate void DialogResultEvent(Wizard_Dialog sender, Wizard_Dialog_Result result);
        public event DialogResultEvent DialogResult;

        protected Button btnCancel = new Button { Text = "Cancel" };
        protected Button btnNext = new Button { Text = ">>" };
        protected Button btnBack = new Button { Text = "<<" };

        public WizardDialogBase()
        {
            this.Resizable = true;
            //Topmost = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            this.AbortButton = btnCancel;
            this.DefaultButton = btnNext;

            ConstructContent();

            btnBack.Click += (s, e) => OnBack();
            btnNext.Click += (s, e) => OnNext();
            btnCancel.Click += (s, e) => OnCancel();
        }

        protected virtual void ConstructContent()
        {
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(ConstructChildContent(), true),
                    new StackLayoutItem(new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Padding = 10,
                        Spacing = 10,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            btnCancel,
                            btnBack,
                            btnNext
                        }
                    })
                }
            };
        }

        protected virtual Control ConstructChildContent()
        {
            return null;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = (Result == Wizard_Dialog_Result.None);
            if (Result != Wizard_Dialog_Result.Abort)
                DialogResult?.Invoke(Wizard_Dialog.Any, Wizard_Dialog_Result.Abort);
        }

        protected virtual void OnCancel()
        {
            DialogResult?.Invoke(Wizard_Dialog.Any, Wizard_Dialog_Result.Cancel);
        }

        protected virtual void OnBack()
        {
            Result = Wizard_Dialog_Result.Back;
            DialogResult?.Invoke(Wizard_Dialog.Any, Wizard_Dialog_Result.Back);
        }

        protected virtual void OnNext()
        {
            Result = Wizard_Dialog_Result.Next;
            DialogResult?.Invoke(Wizard_Dialog.Any, Wizard_Dialog_Result.Next);
        }
    }

    public class SetupWizard
    {
        //Wizard runs the following screens. 
        //Intro
        //Get CLI Tools
        //Set wallet directory
        //Import/Create wallet
        //Questions
        //  Start mining on start
        //  Save Wallet password
        //  Check for updates
        //All other options are defaulted.

        List<WizardDialogBase> pages = new List<WizardDialogBase>();

        int currentPage = 0;

        public SetupWizard()
        {
            AddPage<MainWizardDialog>("NERVA Toolkit Wizard");
            AddPage<GetCliDialog>("Download NERVA");

            if (VersionManager.VersionInfo == null)
                VersionManager.GetVersionInfo();
        }

        private void AddPage<T>(string title) where T : WizardDialogBase, new()
        {
            T page = new T
            {
                Title = title
            };

            page.DialogResult += OnResult;
            pages.Add(page);
        }

        private void ResetPage()
        {
            //todo: we need a mechanism to serialize and deserialize user input
            //so once we reset the page, we can reset the data, so to the user
            //it does not appear that the UI is reset
            WizardDialogBase d = pages[currentPage];
            Type t = d.GetType();

            var v = (WizardDialogBase)Activator.CreateInstance(t);
            v.Title = d.Title;
            v.DialogResult += OnResult;

            pages[currentPage] = v;
        }

        private void OnResult(Wizard_Dialog page, Wizard_Dialog_Result result)
        {
            pages[currentPage].Close(Wizard_Dialog_Result.Abort);

            int newPage = currentPage;
            switch (result)
            {
                case Wizard_Dialog_Result.Cancel:
                case Wizard_Dialog_Result.Abort:
                    {
                        if (MessageBox.Show(Application.Instance.MainForm, "The NERVA Toolkit cannot start until the startup wizard is complete.\r\nAre you sure you wait to quit.", "Wizard Imcomplete",
                            MessageBoxButtons.YesNo, MessageBoxType.Warning, MessageBoxDefaultButton.No) == DialogResult.Yes)
                        Environment.Exit(0);

                        //On linux the state of the close button is wrong if we close and reopen the page
                        //So we need to reset it, but only if the close button is presed
                        if (result == Wizard_Dialog_Result.Abort)
                            ResetPage();
                    }
                    break;
                case Wizard_Dialog_Result.Next:
                    ++newPage;
                    break;
                case Wizard_Dialog_Result.Back:
                    --newPage;
                    break;
                default:
                    Debugger.Break();
                    break;
            }

            
            currentPage = newPage;
            WizardDialogBase d = pages[currentPage];
            
            d.Result = Wizard_Dialog_Result.None;
            d.ShowModal();
        }

        public void Run()
        {
            pages[0].ShowModal();
        }
    }
}