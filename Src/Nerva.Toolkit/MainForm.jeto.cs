using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Json;
using AngryWasp.Logger;

namespace Nerva.Toolkit
{	
	public class MainForm : Form
	{	
		public cons
		public MainForm()
		{
			Log.CreateInstance();
			Log.Instance.Write("NERVA Unified Toolkit. Version {0}", Constants.VERSION);

			JsonReader.Load(this);
		}

		protected void HandleClickMe(object sender, EventArgs e)
		{
			MessageBox.Show("I was clicked!");
		}

		protected void HandleAbout(object sender, EventArgs e)
		{
			new AboutDialog().ShowDialog(this);
		}

		protected void HandleQuit(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
	}
}
