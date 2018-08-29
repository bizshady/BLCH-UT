using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Wizard
{
    public class WizardDialog : Dialog
    {
        protected Button btnCancel = new Button { Text = "Cancel" };
        protected Button btnNext = new Button { Text = ">>" };
        protected Button btnBack = new Button { Text = "<<" };

        private int currentPage = 0;

        private List<WizardContent> pages = new List<WizardContent>();

        public WizardDialog(WizardContent[] pages)
        {
            this.Resizable = true;
            var scr = Screen.PrimaryScreen;
            Location = new Point((int)(scr.WorkingArea.Width - Size.Width) / 2, (int)(scr.WorkingArea.Height - Size.Height) / 2);

            this.AbortButton = btnCancel;
            this.DefaultButton = btnNext;

            btnBack.Click += (s, e) => OnBack();
            btnNext.Click += (s, e) => OnNext();
            btnCancel.Click += (s, e) => OnCancel();

            this.pages = pages.ToList();
            SetButtonsEnabled();
            ConstructContent();
        }

        public void ConstructContent()
        {
            Title = pages[currentPage].Title;

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(new StackLayout
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
                            new StackLayoutItem(pages[currentPage].Content, true),
                        }
                    }, true),
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

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            OnCancel();
        }

        protected virtual void OnCancel()
        {
            if (MessageBox.Show(Application.Instance.MainForm, "The NERVA Toolkit cannot start until the startup wizard is complete.\r\nAre you sure you wait to quit.", "Wizard Imcomplete",
                MessageBoxButtons.YesNo, MessageBoxType.Warning, MessageBoxDefaultButton.No) == DialogResult.Yes)
                Environment.Exit(0);
        }

        protected virtual void OnBack()
        {
            --currentPage;
            if (currentPage < 0)
                currentPage = 0;

            ConstructContent();
            SetButtonsEnabled();
        }

        protected virtual void OnNext()
        {
            ++currentPage;
            if (currentPage >= pages.Count - 1)
                currentPage = pages.Count - 1;

            ConstructContent();
            SetButtonsEnabled();
        }

        private void SetButtonsEnabled()
        {
            btnBack.Enabled = (currentPage > 0);
            btnNext.Enabled = (currentPage < pages.Count - 1);
        }

        public void AllowNavigation(bool allow)
        {
            btnBack.Enabled = allow;
            btnNext.Enabled = allow;
        }
    }
}