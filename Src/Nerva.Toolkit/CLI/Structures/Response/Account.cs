using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Response
{
    [JsonObject]
    public class NewAccount
    {
        [JsonProperty("account_index")]
        public uint Index { get; set; } = uint.MaxValue;
        
        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;
    }

    [JsonObject]
    public class Account
    {
        [JsonProperty("subaddress_accounts")]
        public List<SubAddressAccount> Accounts { get; set; }

        [JsonProperty("total_balance")]
        public ulong TotalBalance { get; set; }

        [JsonProperty("total_unlocked_balance")]
        public ulong TotalUnlockedBalance { get; set; }
    }

    [JsonObject]
    public class SubAddressAccount
    {
        [JsonProperty("account_index")]
        public uint Index { get; set; }

        [JsonProperty("balance")]
        public ulong Balance { get; set; }

        [JsonProperty("base_address")]
        public string BaseAddress { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("unlocked_balance")]
        public ulong UnlockedBalance { get; set; }
    }
}