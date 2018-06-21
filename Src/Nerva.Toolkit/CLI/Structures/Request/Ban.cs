using System.Collections.Generic;
using Nerva.Toolkit.Helpers;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    [JsonObject]
    public class BanList
    {
        [JsonProperty("bans")]
        public List<Ban> Bans { get; set; }
    }

    [JsonObject]
    public class Ban
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("ban")]
        public bool Banned => true;

        [JsonProperty("seconds")]
        public int Seconds => Constants.BAN_TIME;

    }
}