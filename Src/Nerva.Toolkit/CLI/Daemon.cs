using System;
using System.Collections.Generic;
using AngryWasp.Helpers;
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
    /// Minimal Daemon RPC API.
    /// </summary>
    public class DaemonInterface
    {
        private NetHelper netHelper;

        public DaemonInterface()
        {
            netHelper = new NetHelper(Configuration.Instance.Daemon.Rpc);
        }

        /// <summary>
        /// Get the current block height seen by the node
        /// </summary>
        /// <returns></returns>
        public int GetBlockCount()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_block_count"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
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

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_info"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
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

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_connections"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
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

            if (!netHelper.MakeRpcRequest("stop_daemon", null, out result))
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

            string jsonRequest = JsonConvert.SerializeObject(new StartMining
                {
                    BackgroundMining = false,
                    IgnoreBattery = true,
                    MinerAddress = Configuration.Instance.Daemon.MiningAddress,
                    MiningThreads = Configuration.Instance.Daemon.MiningThreads
                });

            //To simplify things we set
            //do_background_mining = false
            //ignore_battery = true
            //string postDataString = $"{{\"do_background_mining\":false,\"ignore_battery\":true,\"miner_address\":\"{Configuration.Instance.Daemon.MiningAddress}\",\"threads_count\":{threads}}}";

            string result = null;

            if (!netHelper.MakeRpcRequest("start_mining", jsonRequest, out result))
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

            if (!netHelper.MakeRpcRequest("mining_status", null, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: mining_status");
                return null;
            }

            return JsonConvert.DeserializeObject<MiningStatus>(result);
        }

        public bool BanPeer(string ip)
        {
            string jsonParams = $"{{\"bans\":[{{\"host\":\"{ip}\",\"ban\":true,\"seconds\":{Constants.BAN_TIME}}}]}}";

            string result = null;

            if (!netHelper.MakeJsonRpcRequest("set_bans", jsonParams, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: set_bans");
                return false;
            }

            var json = JObject.Parse(result);
            return json["result"]["status"].Value<string>().ToLower() == "ok";
        }
    }
}