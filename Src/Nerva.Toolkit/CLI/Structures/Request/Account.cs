using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    [JsonObject]
    public class CreateAccount
    {
        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;
    }

    [JsonObject]
    public class LabelAccount : CreateAccount
    {
        [JsonProperty("account_index")]
        public uint Index { get; set; } = 0;
    }
}