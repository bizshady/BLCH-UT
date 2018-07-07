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

        public static string GetCliExePath(string exe) => $"{Configuration.Instance.ToolsPath}{GetCliExeName(exe)}";

        public static string GetCliExeName(string exe) => (Environment.OSVersion.Platform == PlatformID.Win32NT) ? $"{exe}.exe" : exe;

        public static string GetCliExeBaseName(string exe) => Path.GetFileNameWithoutExtension(exe).ToLower();
    }
}