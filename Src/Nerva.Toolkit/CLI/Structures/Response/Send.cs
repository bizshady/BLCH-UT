using System.Collections.Generic;
using Nerva.Toolkit.CLI.Structures.Response;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Response
{
    [JsonObject]
    public class Send
    {
        [JsonProperty("fee")]
        public ulong Fee { get; set; } = 0;

        [JsonProperty("tx_hash")]
        public string TxHash { get; set; } = string.Empty;

        [JsonProperty("tx_key")]
        public string TxKey { get; set; } = string.Empty;

        [JsonProperty("amount_keys")]
        public string[] AmountKeys { get; set; } = null;

        [JsonProperty("amount")]
        public ulong Amount { get; set; } = 0;
    }
}