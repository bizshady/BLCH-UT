using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Response
{
    [JsonObject]
    public class KeyInfo
    {
        [JsonProperty("public_view_key")]
        public string PublicViewKey { get; set; } = string.Empty;

        [JsonProperty("public_spend_key")]
        public string PublicSpendKey { get; set; } = string.Empty;

        [JsonProperty("private_view_key")]
        public string PrivateViewKey { get; set; } = string.Empty;

        [JsonProperty("private_spend_key")]
        public string PrivateSpendKey { get; set; } = string.Empty;

        [JsonProperty("mnemonic")]
        public string MnemonicSeed { get; set; } = string.Empty;
    }
}