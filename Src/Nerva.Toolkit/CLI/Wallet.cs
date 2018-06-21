using AngryWasp.Logger;
using Nerva.Toolkit.CLI.Structures.Request;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI
{
    /// <summary>
    /// Minimal Wallet RPC API.
    /// </summary>
    public class WalletInterface
    {
        private NetHelper netHelper;

        public WalletInterface()
        {
            netHelper = new NetHelper(Configuration.Instance.Wallet.Rpc);
        }

        public Account GetAccounts()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_accounts"
            };

            //todo: We can optionally add a filter to only get selected subaddresses.
            //This probably isn't necessary though

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_connections");
                return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<Account>>(result).Result;
        }

        public bool CreateWallet(string walletName, string password)
        {
            //todo: need to check for an error being returned. 
            //potentially trying to create a wallet with the same name etc
            string result = null;

            JsonRequest<CreateWallet> jr = new JsonRequest<CreateWallet>
            {
                MethodName = "create_wallet",
                Params = new CreateWallet
                {
                    FileName = walletName,
                    Password = password
                }
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: create_wallet");
                return false;
            }

            return false;
        }

        public bool OpenWallet(string walletName, string password)
        {
            //todo: need to check for an error being returned. 
            //potentially opening a wallet that doesn't exists
            //wrong password etc
            string result = null;

            JsonRequest<OpenWallet> jr = new JsonRequest<OpenWallet>
            {
                MethodName = "open_wallet",
                Params = new OpenWallet
                {
                    FileName = walletName,
                    Password = password
                }
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: open_wallet");
                return false;
            }

            return true;
        }
    }
}