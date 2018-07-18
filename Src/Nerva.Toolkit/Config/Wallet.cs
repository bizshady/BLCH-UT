using System;
using System.IO;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;

namespace Nerva.Toolkit.Config
{
	public class Wallet
	{
        private string walletDir;

		public RpcDetails Rpc { get; set; }

        public string WalletDir
        {
            get => walletDir;

            set
            {
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);

                walletDir = value;
            }
        }

        public string LastOpenedWallet { get; set; }

        public bool SaveWalletPassword { get; set; } = true;

        public string LastWalletPassword { get; set; }

        public int NumTransfersToDisplay { get; set; } = 25;

		public static Wallet New()
        {
            return new Wallet
            {
                WalletDir = Path.Combine(Environment.CurrentDirectory, "Wallets"),
                LastOpenedWallet = null,
                LastWalletPassword = null,
                SaveWalletPassword = false,

                Rpc = RpcDetails.New(MathHelper.Random.NextInt(10000, 50000))
            };
        }
    }
}