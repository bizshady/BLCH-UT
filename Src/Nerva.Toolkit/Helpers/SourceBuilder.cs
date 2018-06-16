using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Helpers
{	
    /// <summary>
    /// Build from source code on Linux.
    /// </summary>
	public static class LinuxSourceBuilder
    {
        public static bool CheckBinaryAvailable()
        {
            return false;
        }

        public static bool CheckLibsInstalled()
        {
            return false;
        }

        public static bool CloneRepo()
        {
            return false;
        }

        public static string GetRelease()
        {
            return "unknown:unknown";
        }
    }
}