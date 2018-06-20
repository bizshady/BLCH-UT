using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    [JsonObject]
    public class JsonRequest
    { 
        private const string RPC_VERSION = "2.0";
        private const string ID = "0";

        [JsonProperty("jsonrpc")]
        public string RpcVersion => RPC_VERSION;

        [JsonProperty("id")]
        public string RpcId => ID;
        
        [JsonProperty("method")]
        public string MethodName { get; set; }

        public string Encode()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [JsonObject]
    public class JsonRequest<T> : JsonRequest
    {  
        [JsonProperty("params")]
        public T Params { get; set; }
    }
}