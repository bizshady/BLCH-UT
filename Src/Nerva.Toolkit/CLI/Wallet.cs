using AngryWasp.Logger;
using Nerva.Toolkit.CLI.Structures.Request;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return null;
            }

            if (CheckError(jr.MethodName, result, true))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<Account>>(result).Result;
        }

        public bool StopWallet()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "stop_wallet"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result);
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
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result);
        }

        public bool OpenWallet(string walletName, string password)
        {
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
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result);
        }

        private bool CheckError(string methodName, string result, bool suppressErrorMessage = false)
        {
            var error = JObject.Parse(result)["error"];

            if (error != null)
            {
                int code = error["code"].Value<int>();
                string message = error["message"].Value<string>();
                if (!suppressErrorMessage)
                    Log.Instance.Write(Log_Severity.Error, "Wallet.{0}: Code {1}, Message: '{2}'", methodName, code, message);
                return true;
            }

            return false;
        }
    }
}