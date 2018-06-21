using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Response
{
    [JsonObject]
    public class JsonValue<T>
    {  
        [JsonProperty("result")]
        public T Result { get; set; }
    }
}