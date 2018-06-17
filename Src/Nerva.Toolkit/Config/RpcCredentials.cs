namespace Nerva.Toolkit.Config
{	
	public class RpcCredentials
    {
        public bool PrivateRpc { get; set; } = true;

        public string RpcLogin { get; set; } = "user";

        public string RpcPass { get; set; } = "pass";
    }
}