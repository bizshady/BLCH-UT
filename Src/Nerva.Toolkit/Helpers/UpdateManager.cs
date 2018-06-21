using AngryWasp.Logger;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.CLI.Structures.Response;

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

        public static void CheckForCliUpdates()
        {
            updateStatus = Update_Status_Code.Undefined;

            Log.Instance.Write("Checking for updates...");
            Info daemonInfo = Cli.Instance.Daemon.GetInfo();

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
            NetHelper.MakeHttpRequest("http://api.getnerva.org/getversion.php", out versionString);

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