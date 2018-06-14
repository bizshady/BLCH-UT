using System;
using System.Collections.Generic;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Nerva.Toolkit.CLI.Structures;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nerva.Toolkit.CLI
{
    public class DaemonInterface
    {
        /// <summary>
        /// Get the current block height seen by the node
        /// </summary>
        /// <returns></returns>
        public int GetBlockCount()
        {
            string result = null;

            if (!NetHelper.MakeJsonRpcRequest("get_block_count", out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_block_count");
                return -1;
            }

            return int.Parse(JObject.Parse(result)["result"]["count"].Value<string>());
        }

        /// <summary>
        /// gets node information
        /// </summary>
        /// <returns>A JObject containing the node information</returns>
        public Info GetInfo()
        {
            string result = null;

            if (!NetHelper.MakeJsonRpcRequest("get_info", out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_info");
                return null;
            }

            //var inf = JObject.Parse(result)["result"];//.Value<Info>();

            var info = JsonConvert.DeserializeObject<JsonValue<Info>>(result);

            return info.Result;
        }

        /// <summary>
        /// gets node connections information
        /// </summary>
        /// <returns>A JObject containing the node information</returns>
        public List<Connection> GetConnections()
        {
            string result = null;

            if (!NetHelper.MakeJsonRpcRequest("get_connections", out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_connections");
                return null;
            }

            var info = JsonConvert.DeserializeObject<JsonValue<ConnectionList>>(result);
            return info.Result.Connections;
        }

        /// <summary>
        /// Stops the CLI daemon
        /// </summary>
        /// <returns>Returns a bool value indicating if the request was successful</returns>
        public bool StopDaemon()
        {
            string result = null;

            if (!NetHelper.MakeRpcRequest("stop_daemon", null, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: stop_daemon");
                return false;
            }

            var json = JObject.Parse(result);
            bool ok = json["status"].Value<string>().ToLower() == "ok";

            return ok;
        }

        /// <summary>
        /// Starts mining on the node
        /// </summary>
        /// <param name="miningThreads">The number of threads to set mining</param>
        /// <returns>Returns a bool value indicating if the request was successful</returns>
        public bool StartMining(int miningThreads)
        {
            int threads = MathHelper.Clamp(miningThreads, 1, Environment.ProcessorCount - 1);

            //To simplify things we set
            //do_background_mining = false
            //ignore_battery = true
            string postDataString = $"{{\"do_background_mining\":false,\"ignore_battery\":true,\"miner_address\":\"{Configuration.Instance.WalletAddress}\",\"threads_count\":{threads}}}";

            string result = null;

            if (!NetHelper.MakeRpcRequest("start_mining", postDataString, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: start_mining");
                return false;
            }

            var json = JObject.Parse(result);
            bool ok = json["status"].Value<string>().ToLower() == "ok";

            return ok;
        }
    }
}