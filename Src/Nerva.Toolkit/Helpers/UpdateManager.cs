using System;
using System.IO;
using System.Net;
using AngryWasp.Logger;
using Nerva.Rpc.Daemon;
using Nerva.Toolkit.CLI;

namespace Nerva.Toolkit.Helpers
{
    public enum Update_Status_Code
    {
        Undefined,
        NewVersionAvailable,
        UpToDate,
    }

	public static class UpdateManager
	{
        //Deprecated. Move all code to VersionManager
        private static Update_Status_Code updateStatus = Update_Status_Code.Undefined;

        public static Update_Status_Code UpdateStatus => updateStatus;

        public static bool MakeHttpRequest(string url, out string returnString)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                using (Stream stream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    returnString = reader.ReadToEnd();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Warning, "Error attempting HTTP request {0}", url);
                Log.Instance.WriteNonFatalException(ex);
                returnString = null;
                return false;
            }
        }

        public static void CheckForCliUpdates()
        {
            updateStatus = Update_Status_Code.Undefined;

            Log.Instance.Write("Checking for updates...");
            GetInfoResponseData daemonInfo = Cli.Instance.Daemon.Interface.GetInfo();

            if (daemonInfo == null)
            {
                Log.Instance.Write(Log_Severity.Error, "Failed to poll version information from daemon");
                return;
            }
            
            var version = daemonInfo.Version;
            
            ulong localVersion = Conversions.OctetSetToInt(version);
            ulong remoteVersion = CheckAvailableVersion();

            if (remoteVersion == 0)
                return;

            Log.Instance.Write("Installed CLI version {0}", version);
            updateStatus = (remoteVersion == localVersion) ? Update_Status_Code.UpToDate : Update_Status_Code.NewVersionAvailable;
        }
        
        private static ulong CheckAvailableVersion()
        {
            string versionString = null;
            MakeHttpRequest("http://api.getnerva.org/getversion.php", out versionString);

            if (versionString == null)
            {
                Log.Instance.Write(Log_Severity.Error, "Could not retrieve available update version");
                return 0;
            }

            Log.Instance.Write("Available CLI version {0}", versionString);
            return Conversions.OctetSetToInt(versionString);
        }
    }
}