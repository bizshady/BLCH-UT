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

        public string CliVersionNumber
        {
            get
            {
                return CliVersion.Split(':')[0].Trim();
            }
        }

        public string CliVersionCodeName
        {
            get
            {
                return CliVersion.Split(':')[1].Trim();
            }
        }

        public string GuiVersionNumber
        {
            get
            {
                return GuiVersion.Split(':')[0].Trim();
            }
        }

        public string GuiVersionCodeName
        {
            get
            {
                return GuiVersion.Split(':')[1].Trim();
            }
        }

        [JsonProperty("binary_url")]
        public string BinaryUrl { get; set; }

        [JsonProperty("windows")]
        public string WindowsLink { get; set; }

        [JsonProperty("gui")]
        public string GuiLink { get; set; }

        [JsonProperty("ubuntu")]
        public Dictionary<string, string> UbuntuLinks { get; set; }

        [JsonProperty("debian")]
        public Dictionary<string, string>  DebianLinks { get; set; }

        [JsonProperty("fedora")]
        public Dictionary<string, string>  FedoraLinks { get; set; }

        [JsonProperty("bootstrap")]
        public string[] BootstrapFiles { get; set; }
    }
}