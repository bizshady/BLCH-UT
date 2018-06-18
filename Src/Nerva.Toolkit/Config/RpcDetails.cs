using AngryWasp.Helpers;

namespace Nerva.Toolkit.Config
{	
	public class RpcDetails
    {
        public bool IsPublic { get; set; } = false;

        public int Port { get; set; } = 21566;

        public string Login { get; set; } = "";

        public string Pass { get; set; } = "";

        public static RpcDetails New()
        {
            RpcDetails d = new RpcDetails
            {
                IsPublic = false,
                Port = MathHelper.Random.NextInt(10000, 50000),
                Login = StringHelper.GenerateRandomString(24),
                Pass = StringHelper.GenerateRandomString(24)
            };

            return d;
        }
    }
}