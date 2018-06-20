using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Response
{
    [JsonObject]
    public class MiningStatus
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("is_background_mining_enabled")]
        public bool IsBackground { get; set; }

        [JsonProperty("speed")]
        public int Speed { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("threads_count")]
        public int ThreadCount { get; set; }
    }
}