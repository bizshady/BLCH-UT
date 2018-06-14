namespace Nerva.Toolkit.Config
{	
	public class Daemon
    {
        public int RpcPort { get; set; } = 17566;

        public bool PrivateRpc { get; set; } = true;

        public string RpcLogin { get; set; } = "user";

        public string RpcPass { get; set; } = "pass";

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = true;

        public int MiningThreads { get; set; } = 6;

        public bool ReconnectToDaemonProcess { get; set; } = true;
    }
}