using AngryWasp.Serializer;

namespace Nerva.Toolkit.Config
{	
	public class Daemon
    {
        public RpcCredentials Credentials { get; set; } = new RpcCredentials();

        public int RpcPort { get; set; } = 20566;

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = true;

        public int MiningThreads { get; set; } = 6;

        public bool ReconnectToDaemonProcess { get; set; } = true;
    }
}