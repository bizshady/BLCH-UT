using System;
using System.Collections.Generic;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Nerva.Rpc.Daemon;
using Nerva.Toolkit.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nerva.Toolkit.CLI
{
    public class DaemonInterface : CliInterface
    {
        public DaemonInterface() : base(Configuration.Instance.Daemon.Rpc) { }
        
        public uint GetBlockCount()
        {
            uint ret = 0;

            new GetBlockCount((uint result) => {
                ret = result;
            }, null, r.Port).Run();

            return ret;
        }

        public GetInfoResponseData GetInfo()
        {
            GetInfoResponseData ret = null;

            new GetInfo((GetInfoResponseData result) => {
                ret = result;
            }, null, r.Port).Run();

            return ret;
        }

        public List<GetConnectionsResponseData> GetConnections()
        {
            List<GetConnectionsResponseData> ret = null;

            new GetConnections((List<GetConnectionsResponseData> result) => {
                ret = result;
            }, null, r.Port).Run();

            return ret;
        }

        public bool StopDaemon()
        {
            return new StopDaemon(null, null, r.Port).Run();
        }

        public bool StartMining()
        {
            //todo: do we need background mining?
            int threads = MathHelper.Clamp(Configuration.Instance.Daemon.MiningThreads, 1, Environment.ProcessorCount);

            return new StartMining(new StartMiningRequestData {
                MinerAddress = Configuration.Instance.Daemon.MiningAddress,
                MiningThreads = threads
            }, null, null, r.Port).Run();
        }

        public bool StopMining()
        {
            return new StopMining(null, null, r.Port).Run();
        }

        public MiningStatusResponseData MiningStatus()
        {
            MiningStatusResponseData ret = null;

            new MiningStatus((MiningStatusResponseData result) => {
                ret = result;
            }, null, r.Port).Run();

            return ret;
        }

        public bool BanPeer(string ip)
        {
            return new SetBans(new SetBansRequestData {
                Bans = new List<Ban> {
                    new Ban {
                        Host = ip
                    }
                }
            }, null, null, r.Port).Run();
        }
    }
}