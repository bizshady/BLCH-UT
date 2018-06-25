using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    /// <summary>
    /// Hacked structure to set some sane defaults to 
    /// get all the useful transfer info
    /// </summary>
    public class GetTransfers
    {
        [JsonProperty("in")]
        public bool In => true;

        [JsonProperty("out")]
        public bool Out => true;

        [JsonProperty("pending")]
        public bool Pending => true;

        [JsonProperty("failed")]
        public bool Failed => false;

        [JsonProperty("pool")]
        public bool Pool => false;
    }
}