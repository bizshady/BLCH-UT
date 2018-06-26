using System;
using System.IO;
using Nerva.Toolkit.Config;

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
    }
}