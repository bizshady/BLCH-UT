using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nerva.Toolkit.Helpers
{
    [JsonObject]
    public class VersionInfo
    {
        [JsonProperty("cli_version")]
        public string CliVersion { get; set; }

        [JsonProperty("gui_version")]
        public string GuiVersion { get; set; }

        [JsonProperty("binary_url")]
        public string BinaryUrl { get; set; }

        [JsonProperty("windows")]
        public string WindowsLink { get; set; }

        [JsonProperty("linux")]
        public string LinuxLink { get; set; }
        
        [JsonProperty("mac")]
        public string MacLink { get; set; }

        [JsonProperty("gui")]
        public string GuiLink { get; set; }

        [JsonProperty("bootstrap")]
        public string[] BootstrapFiles { get; set; }

        public string CliVersionNumber => CliVersion.Split(':')[0].Trim();

        public string CliVersionCodeName => CliVersion.Split(':')[1].Trim();

        public string GuiVersionNumber => GuiVersion.Split(':')[0].Trim();

        public string GuiVersionCodeName => GuiVersion.Split(':')[1].Trim();
    }
}