using System.IO;
using AngryWasp.Logger;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.CLI;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Content.Dialogs
{
	public enum Wallet_Wizard_Result
	{
		Undefined,
		NewWalletCreated,
		WalletOpened,
		WalletImported,
		Cancelled
	}

    public class WalletHelper
	{
        public static bool ShowSavePasswordMessage(string password)
        {
            if (MessageBox.Show(Application.Instance.MainForm, "Would you like to save your wallet password?\r\n" +
				"You will not be asked for your password to open the wallet again\r\n\r\nSECURITY WARNING:\r\n\r\n" +
				"The password is stored in an easily decoded format\r\nAnyone with access to this machine can open your wallet without the password",
                "Save Password?", MessageBoxButtons.YesNo, MessageBoxType.Question, MessageBoxDefaultButton.No ) == DialogResult.Yes)
			{
				Log.Instance.Write(Log_Severity.Warning, "User selected to save password");

				if (string.IsNullOrEmpty(password))
					Configuration.Instance.Wallet.LastWalletPassword = string.Empty;
				else
					Configuration.Instance.Wallet.LastWalletPassword = password.EncodeBase64();

                return true;
			}
			else
			{
				Log.Instance.Write(Log_Severity.Warning, "User selected not to save password. Wiping previously saved password");
				Configuration.Instance.Wallet.LastWalletPassword = null;
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
				string pass = w.LastWalletPassword == string.Empty ? string.Empty : w.LastWalletPassword.DecodeBase64();
				if (Cli.Instance.Wallet.OpenWallet(w.LastOpenedWallet, pass))
				{
					Log.Instance.Write("Wallet file '{0}' opened", w.LastOpenedWallet);
					return true;
				}
				else
				{
					Log.Instance.Write(Log_Severity.Warning, "Wallet cannot be opened from saved information");
					MessageBox.Show(Application.Instance.MainForm, "Wallet cannot be opened from saved information");
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
								//do not ask to save the password here. Only ask once when the wallet is first opened via
								//the OpenWalletDialog
								if (Cli.Instance.Wallet.OpenWallet(w.LastOpenedWallet, d.Password))
									return true;
							}
							break;
						default:
							return false;
					}
				}
            }
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
			var f = GetWalletFiles();

			if (f == null)
				return 0;

			return f.Length;
		}

		public static FileInfo[] GetWalletFiles()
		{
			if (!WalletDirExists())
				return null;

			DirectoryInfo dir = new DirectoryInfo(Configuration.Instance.Wallet.WalletDir);
			return dir.GetFiles("*.keys", SearchOption.TopDirectoryOnly);
		}

		public static void ShowWalletWizard(out Wallet_Wizard_Result result)
		{
			result = Wallet_Wizard_Result.Undefined;

			//We only break this loop if cancel is pressed from the main form
			while (true)
			{
				MainWalletDialog d = new MainWalletDialog();
				switch (d.ShowModal())
				{
					case Open_Wallet_Dialog_Result.Open:
						{ //Open an existing wallet
							while (true)
							{
								OpenWalletDialog d2 = new OpenWalletDialog();
								if (d2.ShowModal() == DialogResult.Ok)
								{
									Configuration.Instance.Wallet.LastOpenedWallet = d2.Name;
									ShowSavePasswordMessage(d2.Password);
									result = Wallet_Wizard_Result.WalletOpened;
									return;
								}
								else //break the loop if cancelled
									break;
							}
						}
						break;
					case Open_Wallet_Dialog_Result.New:
						{ //Create a new wallet
							while (true)
							{
								NewWalletDialog d2 = new NewWalletDialog();
								if (d2.ShowModal() == DialogResult.Ok)
								{
									bool created = Cli.Instance.Wallet.CreateWallet(d2.Name, d2.Password);

									if (created)
									{
										Configuration.Instance.Wallet.LastOpenedWallet = d2.Name;
										ShowSavePasswordMessage(d2.Password);
										result = Wallet_Wizard_Result.NewWalletCreated;
										return;
									}
								}
								else //break the loop if cancelled
									break;
							}
						}
						break;
					case Open_Wallet_Dialog_Result.Import:
						{ //Import a wallet from seed
							MessageBox.Show(Application.Instance.MainForm, "Importing a wallet from seed is not yet supported");
							result = Wallet_Wizard_Result.WalletImported;
							break; //todo: change to return when implemented
						}
					default:
						{
							result = Wallet_Wizard_Result.Cancelled;
							return;	
						}				
				}
			}
		}
    }
}