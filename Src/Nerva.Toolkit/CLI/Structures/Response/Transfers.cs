using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Response
{
    [JsonObject]
    public class TransferList
    {
        [JsonProperty("in")]
        public List<Transfer> Incoming { get; set; }

        [JsonProperty("out")]
        public List<Transfer> Outgoing { get; set; }

        [JsonProperty("pending")]
        public List<Transfer> Pending { get; set; }
    }

    [JsonObject]
    public class Transfer
    {
        [JsonProperty("txid")]
        public string TxId { get; set; }

        [JsonProperty("payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty("height")]
        public uint Height { get; set; }

        [JsonProperty("timestamp")]
        public ulong Timestamp { get; set; }

        [JsonProperty("amount")]
        public ulong Amount { get; set; }

        [JsonProperty("destinations")]
        public List<Destination> Destinations { get; set; }
    }

    [JsonObject]
    public class Destination
    {
        [JsonProperty("amount")]
        public ulong Amount { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }
}