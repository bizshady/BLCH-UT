using System.IO;
using AngryWasp.Logger;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Dialogs
{
    public class WalletHelper
	{
        public static bool ShowSavePasswordMessage(string password)
        {
            if (MessageBox.Show("Would you like to save your wallet password?\r\n" +
				"You will not be asked for your password to open the wallet again\r\n\r\nSECURITY WARNING:\r\n\r\n" +
				"The password is stored in an easily decoded format\r\nAnyone with access to this machine can open your wallet without the password",
                "Save Password?", MessageBoxButtons.YesNo, MessageBoxType.Question, MessageBoxDefaultButton.No ) == DialogResult.Yes)
			{
				Log.Instance.Write(Log_Severity.Warning, "User selected to save password");
				Configuration.Instance.Wallet.LastWalletPassword = password.EncodeBase64();
                return true;
			}
            
            return false;
        }

        public static bool OpenSavedWallet()
        {
			var w = Configuration.Instance.Wallet;

			if (!WalletFileExists(w.LastOpenedWallet))
				return false;

			//Wallet file is saved in config and exists on disk.
			//Load from the saved password if that exists
			if (w.LastWalletPassword != null)
			{
				if (!Cli.Instance.Wallet.OpenWallet(w.LastOpenedWallet, w.LastOpenedWallet.DecodeBase64()))
				{
					Log.Instance.Write(Log_Severity.Warning, "Wallet cannot be opened from saved information");
					MessageBox.Show("Wallet cannot be opened from saved information");
					return false;
				}
			}
            else
            {
				//Keep looping until the right password is entered or cancelled
				while (true)
				{
					EnterPasswordDialog d = new EnterPasswordDialog();
					switch (d.ShowModal())
					{
						case DialogResult.Ok:
							{
								if (Cli.Instance.Wallet.OpenWallet(w.LastOpenedWallet, d.Password))
								{
									ShowSavePasswordMessage(d.Password);
									return true;
								}
							}
							break;
						default:
							return false;
					}
				}
            }

            return false;
        }

		public static bool WalletFileExists(string file)
		{
			if (!WalletDirExists())
				return false;

			if (string.IsNullOrEmpty(file))
				return false;

			string walletFile = Path.Combine(Configuration.Instance.Wallet.WalletDir, file);

			return File.Exists(walletFile);
		}

		public static bool WalletDirExists()
		{
			if (string.IsNullOrEmpty(Configuration.Instance.Wallet.WalletDir))
				return false;

			return Directory.Exists(Configuration.Instance.Wallet.WalletDir);
		}

		public static int GetWalletFileCount()
		{
			if (!WalletDirExists())
				return 0;

			DirectoryInfo dir = new DirectoryInfo(Configuration.Instance.Wallet.WalletDir);
			FileInfo[] files = dir.GetFiles("*.keys", SearchOption.TopDirectoryOnly);
			return files.Length;
		}

		public static bool ShowWalletWizard()
		{
			MainWalletDialog d = new MainWalletDialog();
			switch (d.ShowModal())
			{
				case Open_Wallet_Dialog_Result.Open:
					{ //Open an existing wallet
						//todo: need a dialog to open a wallet from file or select
						//from a list of files stored in the wallets directory
					}
					break;
				case Open_Wallet_Dialog_Result.New:
					{ //Create a new wallet
						NewWalletDialog d2 = new NewWalletDialog();
						if (d2.ShowModal() == DialogResult.Ok)
						{
							string name = d2.Name;
							string password = d2.Password;

							bool created = Cli.Instance.Wallet.CreateWallet(name, password);

							if (created)
							{
								Configuration.Instance.Wallet.LastOpenedWallet = name;
								ShowSavePasswordMessage(password);
							}

							return true;
						}
					}
					break;
				case Open_Wallet_Dialog_Result.Import:
					{ //Import a wallet from seed
						MessageBox.Show("Importing a wallet from seed is not yet supported");
					}
					break;
						
			}

			return false;
		}
    }
}