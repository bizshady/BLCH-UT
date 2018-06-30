using AngryWasp.Serializer;

namespace Nerva.Toolkit.Config
{
	public class Wallet
	{
		public RpcDetails Rpc { get; set; }

        public string WalletDir { get; set; }

        public string LastOpenedWallet { get; set; }

        public bool SaveWalletPassword { get; set; } = true;

        public string LastWalletPassword { get; set; }

		public static Wallet New()
        {
            return new Wallet
            {
                WalletDir = "./wallets",
                LastOpenedWallet = null,
                LastWalletPassword = null,
                SaveWalletPassword = true,
                Rpc = RpcDetails.New()
            };
        }
    }
}