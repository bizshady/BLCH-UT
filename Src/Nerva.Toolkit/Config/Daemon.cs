namespace Nerva.Toolkit.Config
{	
	public class Daemon
    {
        public int RpcPort { get; set; } = 17566;

        public bool PrivateRpc { get; set; } = true;

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = true;

        //Set -1 to automatically select an optimal number of threads
        public int MiningThreads { get; set; } = -1;
    }
}