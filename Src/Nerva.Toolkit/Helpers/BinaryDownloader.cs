using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Helpers
{
    public static class BinaryDownloader
    {
        public static string GetRelease()
        {
            return "unknown:unknown";
        }
        
        public static bool CheckBinaryAvailable()
        {
            return false;
        }
    }	
}