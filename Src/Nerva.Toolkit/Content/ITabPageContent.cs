using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using AngryWasp.Logger;

namespace Nerva.Toolkit.Content
{	
	public interface ITabPageContent
    {
        Control MainControl { get; }
    }
}