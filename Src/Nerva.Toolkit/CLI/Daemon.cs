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
    /// <summary>
    /// Minimal Daemon RPC API.
    /// </summary>
    public class DaemonInterface
    {
        /// <summary>
        /// Get the current block height seen by the node
        /// </summary>
        /// <returns></returns>
        public int GetBlockCount()
        {
            string result = null;

            if (!NetHelper.MakeJsonRpcRequest("get_block_count", null, out result))
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

            if (!NetHelper.MakeJsonRpcRequest("get_info", null, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_info");
                return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<Info>>(result).Result;
        }

        /// <summary>
        /// gets node connections information
        /// </summary>
        /// <returns>A JObject containing the node information</returns>
        public List<Connection> GetConnections()
        {
            string result = null;

            if (!NetHelper.MakeJsonRpcRequest("get_connections", null, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: get_connections");
                return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<ConnectionList>>(result).Result.Connections;
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
            return json["status"].Value<string>().ToLower() == "ok";
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
            return json["status"].Value<string>().ToLower() == "ok";
        }

        public MiningStatus GetMiningStatus()
        {
            string result = null;

            if (!NetHelper.MakeRpcRequest("mining_status", null, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: stop_daemon");
                return null;
            }

            return JsonConvert.DeserializeObject<MiningStatus>(result);
        }

        public bool BanPeer(string ip)
        {
            string jsonParams = $"{{\"bans\":[{{\"host\":\"{ip}\",\"ban\":true,\"seconds\":{Constants.BAN_TIME}}}]}}";

            string result = null;

            if (!NetHelper.MakeJsonRpcRequest("set_bans", jsonParams, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: set_bans");
                return false;
            }

            var json = JObject.Parse(result);
            return json["result"]["status"].Value<string>().ToLower() == "ok";
        }
    }
}