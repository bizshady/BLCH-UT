using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    public class StartMining
    {
        [JsonProperty("do_background_mining")]
        public bool BackgroundMining { get; set; }

        [JsonProperty("ignore_battery")]
        public bool IgnoreBattery { get; set; }

        [JsonProperty("miner_address")]
        public string MinerAddress { get; set; }

        [JsonProperty("threads_count")]
        public int MiningThreads { get; set; }
    }
}