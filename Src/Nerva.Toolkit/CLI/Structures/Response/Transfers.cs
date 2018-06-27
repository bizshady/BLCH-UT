using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Response
{
    [JsonObject]
    public class TransferList
    {
        [JsonProperty("in")]
        public List<Transfer> Incoming { get; set; } = new List<Transfer>();

        [JsonProperty("out")]
        public List<Transfer> Outgoing { get; set; } = new List<Transfer>();

        [JsonProperty("pending")]
        public List<Transfer> Pending { get; set; } = new List<Transfer>();
    }

    [JsonObject]
    public class Transfer
    {
        [JsonProperty("txid")]
        public string TxId { get; set; } = string.Empty;

        [JsonProperty("payment_id")]
        public string PaymentId { get; set; } = string.Empty;

        [JsonProperty("height")]
        public uint Height { get; set; } = 0;

        [JsonProperty("timestamp")]
        public ulong Timestamp { get; set; } = 0;

        [JsonProperty("amount")]
        public ulong Amount { get; set; } = 0;

        [JsonProperty("fee")]
        public ulong Fee { get; set; } = 0;

        [JsonProperty("note")]
        public string Note { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("destinations")]
        public List<Destination> Destinations { get; set; } = new List<Destination>();
    }

    [JsonObject]
    public class TransferTxID : Transfer
    {
        [JsonProperty("unlock_time")]
        public ulong UnlockTime { get; set; } = 0;

        [JsonProperty("subaddr_index")]
        public SubAddressIndex SubAddressIndex { get; set; } = null;

        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;

        [JsonProperty("double_spend_seen")]
        public bool DoubleSpendSeen { get; set; } = false;
    }

    [JsonObject]
    public class TransferContainer
    {
        [JsonProperty("transfer")]
        public TransferTxID Transfer { get; set; }
    }

    [JsonObject]
    public class SubAddressIndex
    {
        [JsonProperty("major")]
        public uint Major { get; set; }

        [JsonProperty("minor")]
        public uint Minor { get; set; }
    }

    [JsonObject]
    public class Destination
    {
        [JsonProperty("amount")]
        public ulong Amount { get; set; } = 0;

        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;
    }
}