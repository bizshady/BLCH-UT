using System;
using System.IO;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Helpers
{
    public static class FileNames
    {
        public const string NERVAD = "blurryd";
        public const string CLI_WALLET = "blurry-wallet-cli";
        public const string RPC_WALLET = "blurry-wallet-rpc";

        public static string GetCliExePath(string exe)
        {
            exe = GetCliExeBaseName(exe);   
            return Path.Combine(Configuration.Instance.ToolsPath, GetFormattedCliExeName(exe));
        }

        public static string GetFormattedCliExeName(string exe)
        {
            exe = GetCliExeBaseName(exe);
            return (OS.Type == OS_Type.Windows) ? $"{exe}.exe" : exe;
        }

        public static string GetCliExeBaseName(string exe) => Path.GetFileNameWithoutExtension(exe).ToLower();

        public static bool DirectoryContainsCliTools(string path)
        {
            DirectoryInfo d = new DirectoryInfo(path);

            if (!d.Exists)
                return false;

            FileInfo[] files = d.GetFiles();

            bool hasDaemon = false;
            bool hasRpcWallet = false;
            bool hasCliWallet = false;

            foreach (var f in files)
            {
                string fn = GetCliExeBaseName(f.FullName);
                switch (fn)
                {
                    case NERVAD:
                        hasDaemon = true;
                    break;
                    case RPC_WALLET:
                        hasRpcWallet = true;
                    break;
                    case CLI_WALLET:
                        hasCliWallet = true;
                    break;
                }

                //early exit if we have found all the cli tools
                if (hasCliWallet && hasRpcWallet && hasDaemon)
                    return true;
            }

            return (hasCliWallet && hasRpcWallet && hasDaemon);
        }
    }
}