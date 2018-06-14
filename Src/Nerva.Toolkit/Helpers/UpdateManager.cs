using System;
using System.Collections.Generic;
using AngryWasp.Logger;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures;
using Newtonsoft.Json.Linq;

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
        private static Update_Status_Code updateStatus = Update_Status_Code.Undefined;

        public static Update_Status_Code UpdateStatus
        {
            get { return updateStatus; }
        }

        private static int ConvertVersionStringToInt(string vs)
        {
            //TODO; Check format. Unhandled exception will occur if wrong string is passed in
            int i = 0;

            string[] split = vs.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            i += int.Parse(split[0]) * 16777216;
            i += int.Parse(split[1]) * 65536;
            i += int.Parse(split[2]) * 256;
            i += int.Parse(split[3]);

            return i;
        }

        public static void CheckForCliUpdates()
        {
            updateStatus = Update_Status_Code.Undefined;

            Log.Instance.Write("Checking for updates...");
			//JObject j = Cli.Instance.Daemon.GetInfo();
            Info daemonInfo = Cli.Instance.Daemon.GetInfo();

            if (daemonInfo == null)
            {
                Log.Instance.Write(Log_Severity.Error, "Failed to poll version information from daemon");
                return;
            }
            
            var version = daemonInfo.Version;
            
            int localVersion = ConvertVersionStringToInt(version);
            int remoteVersion = CheckAvailableVersion();

            if (remoteVersion == -1)
                return;

            Log.Instance.Write("Installed CLI version {0}", version);
            updateStatus = (remoteVersion == localVersion) ? Update_Status_Code.UpToDate : Update_Status_Code.NewVersionAvailable;
        }
        
        private static int CheckAvailableVersion()
        {
            string versionString = null;
            NetHelper.MakeHttpRequest("http://api.getnerva.org/getversion.php", out versionString);

            if (versionString == null)
            {
                Log.Instance.Write(Log_Severity.Error, "Could not retrieve available update version");
                return -1;
            }

            Log.Instance.Write("Available CLI version {0}", versionString);
            return ConvertVersionStringToInt(versionString);
        }
    }
}