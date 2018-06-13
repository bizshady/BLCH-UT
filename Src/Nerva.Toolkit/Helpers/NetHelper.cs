using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Helpers
{	
	public static class NetHelper
	{
        public static string GenerateParams(params KeyValuePair<string, string>[] args)
        {
            return null;
        }

        public static bool MakeJsonRpcRequest(string methodName, out string jsonString)
        {
            int port = Configuration.Instance.Daemon.RpcPort;
            string url = $"http://127.0.0.1:{port}/json_rpc";
            string postDataString = $"{{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"{methodName}\"}}";
            byte[] postData = Encoding.ASCII.GetBytes(postDataString);

            if (Configuration.Instance.LogRpcTraffic)
            {
                Log.Instance.Write(Log_Severity.None, "JSON RPC REQUEST:");
                Log.Instance.Write(Log_Severity.None, url);
                Log.Instance.Write(Log_Severity.None, postDataString);
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json";

            using (Stream stream = req.GetRequestStream())
                stream.Write(postData, 0, postData.Length);

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                using (Stream stream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    jsonString = reader.ReadToEnd();
                }

                if (Configuration.Instance.LogRpcTraffic)
                {
                    Log.Instance.Write(Log_Severity.None, "JSON RPC RESPONSE:");
                    Log.Instance.Write(Log_Severity.None, jsonString);
                }

                return true;
            }

            Log.Instance.Write("JSON RPC ERROR: {0}", resp.StatusCode);
            jsonString = null;
            return false;
        }

        public static bool MakeRpcRequest(string methodName, string postDataString, out string jsonString)
        {
            int port = Configuration.Instance.Daemon.RpcPort;
            string url = $"http://127.0.0.1:{port}/{methodName}";

            if (Configuration.Instance.LogRpcTraffic)
            {
                Log.Instance.Write(Log_Severity.None, "RPC REQUEST:");
                Log.Instance.Write(Log_Severity.None, url);
                Log.Instance.Write(Log_Severity.None, postDataString);
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json";

            if (!string.IsNullOrEmpty(postDataString))
            {
                byte[] postData = Encoding.ASCII.GetBytes(postDataString);

                using (Stream stream = req.GetRequestStream())
                    stream.Write(postData, 0, postData.Length);
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                using (Stream stream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    jsonString = reader.ReadToEnd();
                }

                if (Configuration.Instance.LogRpcTraffic)
                {
                    Log.Instance.Write(Log_Severity.None, "JSON RPC RESPONSE:");
                    Log.Instance.Write(Log_Severity.None, jsonString);
                }

                return true;
            }

            Log.Instance.Write(Log_Severity.None, "JSON RPC ERROR: {0}", resp.StatusCode);
            jsonString = null;
            return false;
        }

        public static bool MakeHttpRequest(string url, out string returnString)
        {
            if (Configuration.Instance.LogRpcTraffic)
            {
                Log.Instance.Write(Log_Severity.None, "HTTP REQUEST:");
                Log.Instance.Write(Log_Severity.None, url);
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                HttpStatusCode status = resp.StatusCode;
                using (Stream stream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    returnString = reader.ReadToEnd();
                }
                
                if (Configuration.Instance.LogRpcTraffic)
                {
                    Log.Instance.Write(Log_Severity.None, "HTTP RESPONSE:");
                    Log.Instance.Write(Log_Severity.None, returnString);
                }

                return true;
            }

            Log.Instance.Write(Log_Severity.None, "HTTP ERROR: {0}", resp.StatusCode);
            returnString = null;
            return false;
        }
    }
}