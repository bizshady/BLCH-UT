using System;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.CLI
{
    public class DaemonInterface
    {
        public string FormatJsonRpcRequest(string methodName)
        {
            int port = Configuration.Instance.DaemonConfig.RpcPort;
            return $"http://127.0.0.1:{port}/json_rpc -d '{{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"{methodName}\"}}' -H 'Content-Type: application/json'";
        }

        public void GetBlockCount()
        {
            string request = FormatJsonRpcRequest("get_block_count");
        }
    }
}