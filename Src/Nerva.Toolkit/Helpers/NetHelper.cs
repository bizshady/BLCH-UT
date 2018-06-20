using System;
using System.IO;
using System.Net;
using System.Text;
using AngryWasp.Logger;
using Nerva.Toolkit.CLI.Structures.Request;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Helpers
{
    public class NetHelper
	{
        private RpcDetails rpc;

        public NetHelper(RpcDetails rpc)
        {
            this.rpc = rpc;
        }

        public bool MakeJsonRpcRequest(JsonRequest request, out string jsonString)
        {
            try
            {
                string url = $"http://127.0.0.1:{rpc.Port}/json_rpc";

                string reqData = request.Encode();
                byte[] reqDataBytes = Encoding.ASCII.GetBytes(reqData);

                if (Configuration.Instance.LogRpcTraffic)
                {
                    Log.Instance.Write(Log_Severity.None, "JSON RPC REQUEST:");
                    Log.Instance.Write(Log_Severity.None, url);
                    Log.Instance.Write(Log_Severity.None, reqData);
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/json";

                using (Stream stream = req.GetRequestStream())
                    stream.Write(reqDataBytes, 0, reqDataBytes.Length);

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

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
            catch (WebException ex)
            {
                Log.Instance.Write(Log_Severity.Error, ex.Message);
                jsonString = null;
                return false;
            }
        }

        public bool MakeJsonRpcRequest(string methodName, string jsonParams, out string jsonString)
        {
            try
            {
                string url = $"http://127.0.0.1:{rpc.Port}/json_rpc";
                string postDataString = $"{{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"{methodName}\"}}";

                if (jsonParams != null)
                    postDataString = $"{{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"{methodName}\",\"params\":{jsonParams}}}";

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
            catch (WebException ex)
            {
                Log.Instance.Write(Log_Severity.Error, ex.Message);
                jsonString = null;
                return false;
            }
        }

        public bool MakeRpcRequest(string methodName, string postDataString, out string jsonString)
        {
            try
            {
                string url = $"http://127.0.0.1:{rpc.Port}/{methodName}";
   
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
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, ex.Message);
                jsonString = null;
                return false;
            }
        }

        public bool MakeHttpRequest(string url, out string returnString)
        {
            try
            {
                if (Configuration.Instance.LogRpcTraffic)
                {
                    Log.Instance.Write(Log_Severity.None, "HTTP REQUEST:");
                    Log.Instance.Write(Log_Severity.None, url);
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

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
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, ex.Message);
                returnString = null;
                return false;
            }
        }
    }
}