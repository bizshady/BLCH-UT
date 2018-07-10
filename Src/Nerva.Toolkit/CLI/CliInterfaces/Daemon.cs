using System;
using System.Collections.Generic;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Nerva.Toolkit.CLI.Structures.Request;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nerva.Toolkit.CLI
{
    public class DaemonInterface : CliInterface
    {
        public DaemonInterface() : base(Configuration.Instance.Daemon.Rpc) { }
        
        public int GetBlockCount()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_block_count"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);

                return -1;
            }

            return JObject.Parse(result)["result"]["count"].Value<int>();
        }

        public Info GetInfo()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_info"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);

                return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<Info>>(result).Result;
        }

        public List<Connection> GetConnections()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_connections"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
               if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);

                return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<ConnectionList>>(result).Result.Connections;
        }

        public bool StopDaemon()
        {
            string result = null;

            if (!netHelper.MakeRpcRequest("stop_daemon", null, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: stop_daemon");

                return false;
            }

            return JObject.Parse(result)["status"].Value<string>().ToLower() == "ok";
        }

        public bool StartMining()
        {
            int threads = MathHelper.Clamp(Configuration.Instance.Daemon.MiningThreads, 1, Environment.ProcessorCount);

            string jsonRequest = JsonConvert.SerializeObject(new StartMining
                {
                    BackgroundMining = false,
                    IgnoreBattery = true,
                    MinerAddress = Configuration.Instance.Daemon.MiningAddress,
                    MiningThreads = threads
                });
                
            string result = null;

            if (!netHelper.MakeRpcRequest("start_mining", jsonRequest, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: start_mining");

                return false;
            }

            return JObject.Parse(result)["status"].Value<string>().ToLower() == "ok";
        }

        public bool StopMining()
        {
            string result = null;

            if (!netHelper.MakeRpcRequest("stop_mining", null, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: stop_mining");

                return false;
            }

            return JObject.Parse(result)["status"].Value<string>().ToLower() == "ok";
        }

        public MiningStatus GetMiningStatus()
        {
            string result = null;

            if (!netHelper.MakeRpcRequest("mining_status", null, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: mining_status");

                return null;
            }

            return JsonConvert.DeserializeObject<MiningStatus>(result);
        }

        public bool BanPeer(string ip)
        {
            JsonRequest<BanList> jr = new JsonRequest<BanList>
            {
                MethodName = "set_bans",
                Params = new BanList
                {
                    Bans = new List<Ban>(new Ban[] {
                        new Ban
                        {
                            Host = ip
                        }})
                }
            };

            string result = null;

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete RPC call: set_bans");
                    
                return false;
            }

            bool ok = JObject.Parse(result)["result"]["status"].Value<string>().ToLower() == "ok";

            if (ok)
                Log.Instance.Write("Peer {0} banned", ip);

            return ok;
        }
    }
}