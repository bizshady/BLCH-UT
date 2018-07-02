using System;
using System.IO;
using Nerva.Toolkit.Config;
using AngryWasp.Logger;

namespace Nerva.Toolkit.Helpers
{	
	public static class FileNames
    {
        public const string NERVAD = "nervad";
        public const string CLI_WALLET = "nerva-wallet-cli";
        public const string RPC_WALLET = "nerva-wallet-rpc";

        public static string GetCliExePath(string exe)
        {
            string exeName = GetCliExeName(exe);
            string exePath = $"{Configuration.Instance.ToolsPath}{exeName}";
            return exePath;
        }

        public static string GetCliExeName(string exe)
        {
            string exeName = (Environment.OSVersion.Platform == PlatformID.Win32NT) ? $"{exe}.exe" : exe;
            return exeName;
        }

        public static string GetCliExeBaseName(string exe)
        {
            string exeName = Path.GetFileNameWithoutExtension(exe).ToLower();
            return exeName;
        }

        public static string RenameDuplicateFile(string p)
        {
            if (!File.Exists(p))
                return p;

            string fileName = Path.GetFileNameWithoutExtension(p);
            string dirName = Path.GetDirectoryName(p);
            string ext = Path.GetExtension(p);

            for (int n = 0; n < 1000; n++)
            {
                string v = string.Format("{0:000}", n);
                /*if (n < 10)
                    v = "0" + v;
                if (n < 100)
                    v = "0" + v;*/

                string fn = $"{Path.Combine(dirName, fileName)}_{v}{ext}";

                if (!File.Exists(fn))
                    return Path.Combine(dirName, Path.GetFileName(fn));
            }

            //numeric increment failed after 1000 attempts. Crash with error and make user clean up files
            //we set return value to p to prevent compiler error, but this log message will crash the program anyway
            Log.Instance.Write(Log_Severity.Fatal, "Could not rename duplicate file. 1000 attempts failed. Clean up your old files");
            return p;
        }
    }
}