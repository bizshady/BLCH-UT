using AngryWasp.Serializer;

namespace Nerva.Toolkit.Config
{
	public class Wallet
	{
		public RpcDetails Rpc { get; set; }

        public string WalletDir { get; set; }

        public string LastOpenedWallet { get; set; }

        public string LastWalletPassword { get; set; }

		public static Wallet New()
        {
            return new Wallet
            {
                WalletDir = "./wallets",
                LastOpenedWallet = null,
                LastWalletPassword = null,
                Rpc = RpcDetails.New()
            };
        }
    }
}