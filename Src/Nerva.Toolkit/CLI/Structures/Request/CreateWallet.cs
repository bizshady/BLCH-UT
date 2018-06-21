using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures.Request
{
    public class CreateWallet
    {
        private const string LANGUAGE = "English";

        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("language")]
        public string Language => LANGUAGE;
    }
}