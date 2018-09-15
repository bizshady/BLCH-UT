using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
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
            catch (Exception ex)
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.WriteNonFatalException(ex);

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
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.WriteNonFatalException(ex);
                    
                jsonString = null;
                return false;
            }
        }

        public static bool MakeHttpRequest(string url, out string returnString)
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
                Log.Instance.Write(Log_Severity.Warning, "Error attempting HTTP request {0}", url);
                Log.Instance.WriteNonFatalException(ex);
                returnString = null;
                return false;
            }
        }

        public static bool PingServer(string host)
        {
            try
            {
                Ping ping = new Ping();
                IPAddress ip = IPAddress.Parse(host);
                PingReply reply = ping.Send(ip);
                
                if (reply.Status == IPStatus.Success)
                    return true;

                Log.Instance.Write(Log_Severity.Warning, "Failed to ping {0} for online check", host);
                return false;
            }
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Warning, "Error attempting to ping {0} for online check", host);
                Log.Instance.WriteNonFatalException(ex);
                return false;
            }
        }
    }
}