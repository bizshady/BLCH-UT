using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    public class GetTransferByTxID
    {
        [JsonProperty("txid")]
        public string TxID { get; set; }
    }
}