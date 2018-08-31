using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.AccessControl;
using AngryWasp.Logger;
using Mono.Unix.Native;
using Newtonsoft.Json;

namespace Nerva.Toolkit.Helpers
{
    public enum Supported_Systems
    {
        Ubuntu,
        Fedora,
        Debian
    }

    public static class VersionManager
    {
        private static VersionInfo versionInfo;

        public static VersionInfo VersionInfo => versionInfo;

        public static void GetVersionInfo(ulong currentVersion, Action completeAction)
        {
            TaskFactory.Instance.RunTask("checkversions", "Checking for available versions", () =>
            {
                Log.Instance.Write("Checking for new versions");

                string versionString = null;
                NetHelper.MakeHttpRequest("http://getnerva.org/getbinaries.php", out versionString);

                if (versionString == null)
                {
                    //todo: Show a message box
                    Log.Instance.Write(Log_Severity.Error, "Could not retrieve update information");
                    completeAction();
                    return;
                }

                versionInfo = JsonConvert.DeserializeObject<VersionInfo>(versionString);
                ulong newCliVersion = Conversions.OctetSetToInt(versionInfo.CliVersionNumber.TrimStart('v'));
                ulong newGuiVersion = Conversions.OctetSetToInt(versionInfo.GuiVersionNumber.TrimStart('v'));

                if (newCliVersion > currentVersion)
                    Log.Instance.Write("New CLI version available: '{0}'", versionInfo.CliVersion);
                else
                    Log.Instance.Write("CLI tools up to date");

                ulong currentGuiVersion = Conversions.OctetSetToInt(Constants.VERSION);
                if (newGuiVersion > currentGuiVersion)
                    Log.Instance.Write("New GUI version available: '{0}'", versionInfo.GuiVersion);
                else
                    Log.Instance.Write("GUI up to date");

                completeAction();
            });
        }

        public static void DownloadFile(string file, Action<DownloadProgressChangedEventArgs> onProgress, Action<bool, string> onComplete)
        {
            TaskFactory.Instance.RunTask("downloadcli", "Downloading the CLI tools", () =>
            {
                string url = $"{VersionInfo.BinaryUrl}binaries/{file}";
                string destDir = Path.Combine(Environment.CurrentDirectory, "CLI");

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                string destFile = Path.Combine(destDir, file);

                if (File.Exists(destFile))
                {
                    Log.Instance.Write("Local copy of CLI tools found.");
                    ExtractFile(destDir, destFile, onComplete);
                }
                else
                {
                    Log.Instance.Write("Downloading CLI tools.");
                    using (var client = new WebClient())
                    {
                        client.DownloadProgressChanged += (s, e) =>
                        {
                            onProgress(e);
                        };

                        client.DownloadFileCompleted += (s, e) =>
                        {
                            if (e.Error == null)
                                ExtractFile(destDir, destFile, onComplete);
                            else
                            {
                                Log.Instance.WriteNonFatalException(e.Error);
                                onComplete(false, null);
                            }
                        };

                        client.DownloadFileAsync(new Uri(url),  destFile);
                    }
                }
            });   
        }

        private static void ExtractFile(string destDir, string destFile, Action<bool, string> onComplete)
        {
            try
            {
                CLI.Cli.Instance.KillCliProcesses("nervad");
                CLI.Cli.Instance.KillCliProcesses("nerva-wallet-cli");
                CLI.Cli.Instance.KillCliProcesses("nerva-wallet-rpc");

                Log.Instance.Write("Extracting CLI tools");
                ZipArchive archive = ZipFile.Open(destFile, ZipArchiveMode.Read);
                foreach (var a in archive.Entries)
                {
                    string extFile = Path.Combine(destDir, a.FullName);
                    Log.Instance.Write("Extracting {0}", extFile);
                    a.ExtractToFile(extFile, true);

                    // Hack: ZipFile does not maintain permissions on linux. Set the following                      
                    // S_IFREG | S_IRGRP | S_IROTH | S_IRUSR | S_IWUSR | S_IXGRP | S_IXOTH | S_IXUSR
                    if (OS.Type == OS_Type.Linux)
                        Syscall.chmod(extFile, (FilePermissions)33261);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNonFatalException(ex);
                onComplete(false, null);
                return;
            }
            
                            
            onComplete(true, destDir);
        }
    }
}