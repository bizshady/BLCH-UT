using System;
using System.IO;
using System.Net;
using System.Text;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;

namespace Nerva.Toolkit.Helpers
{
    public class NetHelper
	{
        private int port;

        public NetHelper(int port)
        {
            this.port = port;
        }

        public bool MakeJsonRpcRequest(string methodName, string jsonParams, out string jsonString)
        {
            try
            {
                string url = $"http://127.0.0.1:{port}/json_rpc";

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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, ex.Message);
                returnString = null;
                return false;
            }
        }
    }
}