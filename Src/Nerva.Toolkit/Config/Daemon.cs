using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Config
{
    public class Daemon
    {
        public RpcDetails Rpc { get; set; }

        public bool StopOnExit { get; set; }

        public bool AutoStartMining { get; set; }

        public string MiningAddress { get; set; }

        public int MiningThreads { get; set; }

        public static Daemon New(bool testnet)
        {
            return new Daemon
            {
                StopOnExit = false,
                AutoStartMining = false,
                MiningAddress = Constants.DEV_WALLET_ADDRESS,
                MiningThreads = 2,

                Rpc = RpcDetails.New(testnet ? 18566u : 17566u)
            };
        }
    }
}