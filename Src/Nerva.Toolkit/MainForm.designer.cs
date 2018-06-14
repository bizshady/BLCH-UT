using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;

namespace Nerva.Toolkit
{	
	public partial class MainForm : Form
	{	
		public void ConstructLayout()
		{
			Title = "NERVA Toolkit";
			ClientSize = new Size(640, 480);

			Content = new DaemonInfo();

			// create a few commands that can be used for the menu and toolbar
			var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
			clickMe.Executed += HandleClickMe;

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += HandleQuit;

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += HandleAbout;

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem
					{ 
						Text = "&File",
						Items =
						{ 
							clickMe,
							clickMe 
						}
					},
					new ButtonMenuItem
					{
						Text = "&Edit",
						Items = { /* commands/items */ }
					},
					new ButtonMenuItem
					{
						Text = "&View",
						Items =
						{ /* commands/items */ 
						}
					},
				},
				ApplicationItems =
				{
					new ButtonMenuItem
					{
						Text = "&Preferences...",
						Items =
						{
							clickMe
						}
					},
					new ButtonMenuItem
					{
						Text = "&Whatever..."
					},
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
			// create toolbar			
			ToolBar = new ToolBar
			{
				Items =
				{
					clickMe,
					clickMe,
					clickMe
				} 
			};
			
		}
	}
}
