using System;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Wizard
{
    public abstract class WizardDialogBase<T> : Dialog<T>
    {
        protected Button btnCancel = new Button { Text = "Cancel" };
        protected Button btnNext = new Button { Text = ">>" };
        protected Button btnBack = new Button { Text = "<<" };

        public WizardDialogBase(string title)
        {
            this.Title = title;

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

        protected abstract Control ConstructChildContent();

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            ShouldQuit();
        }

        protected virtual void OnCancel()
        {
            ShouldQuit();
        }

        protected abstract void OnBack();
        protected abstract void OnNext();

        private void ShouldQuit()
        {
            if (MessageBox.Show(this, "The NERVA Toolkit cannot start until the startup wizard is complete.\r\nAre you sure you wait to quit.", "Wizard Imcomplete",
                MessageBoxButtons.YesNo, MessageBoxType.Warning, MessageBoxDefaultButton.No) == DialogResult.Yes)
                Environment.Exit(0);
        }
    }

    public class SetupWizard
    {
        public SetupWizard()
        {

        }

        public void Run()
        {
            MainWizardDialog dlg = new MainWizardDialog("NERVA Toolkit Wizard");
            dlg.ShowModal();
        }
    }
}