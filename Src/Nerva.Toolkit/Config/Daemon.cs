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
                AutoStartMining = true,
                MiningAddress = Constants.DEV_WALLET_ADDRESS,
                MiningThreads = 4,

                Rpc = RpcDetails.New(testnet ? 18566 : 17566)
            };
        }
    }
}