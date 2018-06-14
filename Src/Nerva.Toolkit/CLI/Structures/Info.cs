using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI.Structures
{
    [JsonObject]
    public class Info
    {
        [JsonProperty("alt_block_count")]
        public int AltBlockCount { get; set; }

        [JsonProperty("block_size_limit")]
        public int BlockSizeLimit { get; set; }

        [JsonProperty("block_size_median")]
        public int BlockSizeMedian { get; set; }

        [JsonProperty("bootstrap_daemon_address")]
        public string BootstrapDaemonAddress { get; set; }

        [JsonProperty("cumulative_difficulty")]
        public long CumulativeDifficulty { get; set; }

        [JsonProperty("difficulty")]
        public long Difficulty { get; set; }

        [JsonProperty("free_space")]
        public long FreeSpace { get; set; }

        [JsonProperty("grey_peerlist_size")]
        public int GreyPeerListSize { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("height_without_bootstrap")]
        public long HeightWithoutBootstrap { get; set; }

        [JsonProperty("incoming_connections_count")]
        public int IncomingConnectionsCount { get; set; }

        [JsonProperty("mainnet")]
        public bool Mainnet { get; set; }

        [JsonProperty("offline")]
        public bool Offline { get; set; }

        [JsonProperty("outgoing_connections_count")]
        public int OutgoingConnectionsCount { get; set; }

        [JsonProperty("rpc_connections_count")]
        public int RpcConnectionsCount { get; set; }

        [JsonProperty("stagenet")]
        public bool Stagenet { get; set; }

        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("target")]
        public int Target { get; set; }

        [JsonProperty("target_height")]
        public long TargetHeight { get; set; }

        [JsonProperty("testnet")]
        public bool Testnet { get; set; }

        [JsonProperty("top_block_hash")]
        public string TopBlockHash { get; set; }

        [JsonProperty("tx_count")]
        public int TxCount { get; set; }

        [JsonProperty("tx_pool_size")]
        public int TxPoolSize { get; set; }

        [JsonProperty("untrusted")]
        public bool Untrusted { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("was_bootstrap_ever_used")]
        public bool WasBootstrapEverUsed { get; set; }

        [JsonProperty("white_peerlist_size")]
        public int WhitePeerListSize { get; set; }
    }
}