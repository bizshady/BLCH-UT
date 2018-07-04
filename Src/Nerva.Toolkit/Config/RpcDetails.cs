using AngryWasp.Helpers;

namespace Nerva.Toolkit.Config
{	
	public class RpcDetails
    {
        public bool IsPublic { get; set; } = false;

        public int Port { get; set; } = -1;

        public string Login { get; set; } = "";

        public string Pass { get; set; } = "";

        public static RpcDetails New(int port)
        {
            return new RpcDetails
            {
                IsPublic = false,
                Port = port,
                Login = StringHelper.GenerateRandomString(24),
                Pass = StringHelper.GenerateRandomString(24)
            };
        }
    }
}