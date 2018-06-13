using System;
using System.Collections.Generic;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nerva.Toolkit.CLI
{
    public class DaemonInterface
    {
        /// <summary>
        /// gets node information
        /// </summary>
        /// <returns>A JObject containing the node information</returns>
        public JObject GetInfo()
        {
            string result = null;

            if (!NetHelper.MakeJsonRpcRequest("get_info", out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_info");
                return null;
            }

            return JObject.Parse(result);
        }

        /// <summary>
        /// Stops the CLI daemon
        /// </summary>
        /// <returns>Returns a bool value indicating if the request was successful</returns>
        public bool StopDaemon()
        {
            string result = null;

            if (!NetHelper.MakeRpcRequest("stop_daemon", out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: stop_daemon");
                return false;
            }

            var json = JObject.Parse(result);
            bool ok = json["status"].Value<string>().ToLower() == "ok";

            return ok;
        }
    }
}