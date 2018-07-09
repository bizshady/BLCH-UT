using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.CLI
{
    public class CliInterface
    {
        protected NetHelper netHelper;

        public CliInterface(RpcDetails r)
        {
            netHelper = new NetHelper(r);
        }
    }
}