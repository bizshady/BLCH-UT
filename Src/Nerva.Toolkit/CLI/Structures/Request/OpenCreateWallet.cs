using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    public class OpenWallet
    {
        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public class CreateWallet : OpenWallet
    {
        private const string LANGUAGE = "English";

        [JsonProperty("language")]
        public string Language => LANGUAGE;
    }
}