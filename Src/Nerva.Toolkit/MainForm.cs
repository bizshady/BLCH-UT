using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;

namespace Nerva.Toolkit
{
    public partial class MainForm : Form
	{	
		public MainForm()
		{
			SuspendLayout();
			ConstructLayout();
			ResumeLayout();
		}

		protected void HandleClickMe(object sender, EventArgs e)
		{
			MessageBox.Show("I was clicked!");
		}

		protected void HandleAbout(object sender, EventArgs e)
		{
			AboutDialog ad = new AboutDialog();
			ad.ProgramName = "NERVA Unified Toolkit";
			ad.ProgramDescription = "Unified frontend for the NERVA CLI tools";
			string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			ad.Title = "About NERVA Toolkit";
			ad.License = "Copyright © 2018 Angry Wasp";
			ad.Logo = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("Nerva.Toolkit.NERVA-Logo.png"));
			ad.ShowDialog(this);
		}

		protected void HandleQuit(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
    }
}