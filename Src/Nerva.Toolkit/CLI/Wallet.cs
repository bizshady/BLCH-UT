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

            //TODO: We can optionally add a filter to only get selected subaddresses.
            //This probably isn't necessary though

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_connections");
                return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<Account>>(result).Result;
        }
    }
}