using AngryWasp.Serializer;

namespace Nerva.Toolkit.Config
{
	public class Wallet
	{
		public RpcDetails Rpc { get; set; }

        public bool StopOnExit { get; set; }

        public string WalletDir { get; set; }

		public static Wallet New()
        {
            return new Wallet
            {
                StopOnExit = false,
                WalletDir = "./wallets",
                Rpc = RpcDetails.New()
            };
        }
    }
}