using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    public class QueryKey
    {
        [JsonProperty("key_type")]
        public string KeyType { get; set; }
    }
}