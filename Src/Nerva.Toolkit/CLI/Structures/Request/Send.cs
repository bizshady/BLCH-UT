using System.Collections.Generic;
using Nerva.Toolkit.CLI.Structures.Response;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    public enum transfer_Priority
    {
        Default,
        Low,
        Medium,
        High
    }

    public class TransferWithoutPaymentID
    {
        [JsonProperty("destinations")]
        public List<Destination> Destinations { get; set; } = new List<Destination>();

        [JsonProperty("account_index")]
        public uint AccountIndex { get; set; } = 0;

        [JsonProperty("mixin")]
        public uint Mixin => 5;
        
        [JsonProperty("get_tx_key")]
        public bool GetTxKey => true;

        [JsonProperty("priority")]
        public uint Priority { get; set; }
    }

    public class TransferWithPaymentID : TransferWithoutPaymentID
    {
        [JsonProperty("payment_id")]
        public string PaymentId { get; set; }
    }
}