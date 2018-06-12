using System;
using Eto.Forms;

namespace Nerva.Toolkit.Frontend
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(Eto.Platform.Detect).Run(new MainForm());
		}
	}
}
