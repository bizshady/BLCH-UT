using AngryWasp.Serializer;

namespace Nerva.Toolkit.Config
{
	public class Wallet
	{
		public RpcCredentials Credentials { get; set; } = new RpcCredentials();

		public int RpcPort { get; set; } = 21566;
  }
}