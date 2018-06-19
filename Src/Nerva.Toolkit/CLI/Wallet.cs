using System;
using AngryWasp.Logger;
using Nerva.Toolkit.CLI.Structures;
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

            //TODO: We can optionally add a filter to only get selected subaddresses.
            //This probably isn't necessary though

            if (!netHelper.MakeJsonRpcRequest("get_accounts", null, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_connections");
                return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<Account>>(result).Result;
        }
    }
}