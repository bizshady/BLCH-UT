using AngryWasp.Logger;
using Newtonsoft.Json;

namespace Nerva.Toolkit.Helpers
{
    public enum Supported_Systems
    {
        Windows,
        Ubuntu,
        Fedora,
        Debian
    }

    public static class VersionManager
    {
        private static VersionInfo versionInfo;

        public static VersionInfo VersionInfo => versionInfo;

        public static void GetVersionInfo()
        {
            TaskFactory.Instance.RunTask("checkversions", "Checking for available versions", () =>
            {
                string versionString = null;
                NetHelper.MakeHttpRequest("http://getnerva.org/getbinaries.php", out versionString);

                if (versionString == null)
                {
                    //todo: Show a message box
                    Log.Instance.Write(Log_Severity.Error, "Could not retrieve update information");
                    return;
                }

                versionInfo = JsonConvert.DeserializeObject<VersionInfo>(versionString);
            });
        }
    }
}