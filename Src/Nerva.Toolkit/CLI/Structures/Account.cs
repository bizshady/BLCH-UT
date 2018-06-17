using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures
{
     [JsonObject]
    public class Account
    {
        [JsonProperty("subaddress_accounts")]
        public List<SubAddressAccount> Accounts { get; set; }

        [JsonProperty("total_balance")]
        public uint TotalBalance { get; set; }

        [JsonProperty("total_unlocked_balance")]
        public uint TotalUnlockedBalance { get; set; }
    }

    [JsonObject]
    public class SubAddressAccount
    {
        [JsonProperty("account_index")]
        public uint Index { get; set; }

        [JsonProperty("balance")]
        public uint Balance { get; set; }

        [JsonProperty("base_address")]
        public string BaseAddress { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("unlocked_balance")]
        public uint UnlockedBalance { get; set; }
    }
}