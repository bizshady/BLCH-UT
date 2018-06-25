using System;
using System.Diagnostics;
using System.IO;
using AngryWasp.Logger;
using Nerva.Toolkit.CLI.Structures.Request;
using Nerva.Toolkit.CLI.Structures.Response;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nerva.Toolkit.CLI
{
    public enum Key_Type
    {
        View_Key,
        Mnemonic
    }

    /// <summary>
    /// Minimal Wallet RPC API.
    /// </summary>
    public class WalletInterface
    {
        private NetHelper netHelper;

        public WalletInterface()
        {
            netHelper = new NetHelper(Configuration.Instance.Wallet.Rpc);
        }

        public Account GetAccounts()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "get_accounts"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return null;
            }

            if (CheckError(jr.MethodName, result, true))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<Account>>(result).Result;
        }

        public bool StopWallet()
        {
            string result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = "stop_wallet"
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result);
        }

        public bool CreateWallet(string walletName, string password)
        {
            //todo: need to check for an error being returned. 
            //potentially trying to create a wallet with the same name etc
            string result = null;

            JsonRequest<CreateWallet> jr = new JsonRequest<CreateWallet>
            {
                MethodName = "create_wallet",
                Params = new CreateWallet
                {
                    FileName = walletName,
                    Password = password
                }
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result);
        }

        public bool OpenWallet(string walletName, string password)
        {
            string result = null;

            JsonRequest<OpenWallet> jr = new JsonRequest<OpenWallet>
            {
                MethodName = "open_wallet",
                Params = new OpenWallet
                {
                    FileName = walletName,
                    Password = password
                }
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result);
        }

        public string QueryKey(Key_Type keyType)
        {
            string result = null;

            JsonRequest<QueryKey> jr = new JsonRequest<QueryKey>
            {
                MethodName = "query_key",
                Params = new QueryKey
                {
                    KeyType = keyType.ToString().ToLower()
                }
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return null;
            }

            if (CheckError(jr.MethodName, result))
                return null;

            return JObject.Parse(result)["result"]["key"].Value<string>();
        }

        public TransferList GetTransfers(uint scanFromHeight, out uint lastTxHeight)
        {
            string result = null;
            lastTxHeight = 0;

            JsonRequest<GetTransfers> jr = new JsonRequest<GetTransfers>
            {
                MethodName = "get_transfers",
                Params = new GetTransfers
                {
                    ScanFromHeight = scanFromHeight
                }
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return null;
            }

            if (CheckError(jr.MethodName, result))
                return null;

            var txl = JsonConvert.DeserializeObject<JsonValue<TransferList>>(result).Result;

            uint i = 0, o = 0;

            if (txl.Incoming != null && txl.Incoming.Count > 0)
                i = txl.Incoming[txl.Incoming.Count - 1].Height;
            
            if (txl.Outgoing != null && txl.Outgoing.Count > 0)
                o = txl.Outgoing[txl.Outgoing.Count - 1].Height;

            lastTxHeight = Math.Max(i, o);

            return txl;
        }

        private bool CheckError(string methodName, string result, bool suppressErrorMessage = false)
        {
            var error = JObject.Parse(result)["error"];

            if (error != null)
            {
                int code = error["code"].Value<int>();
                string message = error["message"].Value<string>();
                if (!suppressErrorMessage)
                    Log.Instance.Write(Log_Severity.Error, "Wallet.{0}: Code {1}, Message: '{2}'", methodName, code, message);
                return true;
            }

            return false;
        }

        [JsonObject]
        private class RestoreJson
        {
            [JsonProperty("version")]
            public int Version => 1;

            [JsonProperty("filename")]
            public string FileName { get; set; } = string.Empty;

            [JsonProperty("password")]
            public string Password { get; set; } = string.Empty;

            [JsonProperty("scan_from_height")]
            public int ScanHeight => 0;
        }

        [JsonObject]
        private class RestoreJsonKeys : RestoreJson
        {
            [JsonProperty("viewkey")]
            public string View_Key { get; set; } = string.Empty;

            [JsonProperty("spendkey")]
            public string SpendKey { get; set; } = string.Empty;
        }

        [JsonObject]
        private class RestoreJsonSeed : RestoreJson
        {
            [JsonProperty("seed")]
            public string Seed { get; set; } = string.Empty;

            [JsonProperty("seed_passphrase")]
            public string Passphrase { get; set; } = string.Empty;
        }

        public void RestoreWalletFromSeed(string walletName, string seed, string pass)
        {
            //The RPC wallet does not contain a function to restore a wallet from seed,
            //So we have to do some shady shit and call nerva-wallet-cli to restore the wallet for us

            string cliPath = Path.Combine(Configuration.Instance.ToolsPath, "nerva-wallet-cli");
            string walletPath = Path.Combine(Configuration.Instance.Wallet.WalletDir, walletName);
            string jsonPath = $"{walletPath}.json";
            string daemon = $"127.0.0.1:{Configuration.Instance.Daemon.Rpc.Port}";

            var r = new RestoreJsonSeed
            {
                FileName = walletPath,
                Password = pass,
                Seed = seed,
            };

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            var serializer = JsonSerializer.Create(settings);

            using (var sw = new StreamWriter(File.OpenWrite(jsonPath)))
                using (var jsonTextWriter = new JsonTextWriter(sw))
                    serializer.Serialize(jsonTextWriter, r);

            string exe = cliPath;
            string args = $"--daemon-address {daemon} --generate-from-json {jsonPath}";

            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo(exe, args)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };

            proc.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null)
                    return;

                    Log.Instance.Write(e.Data);

                if (e.Data.Contains("Refresh done"))
                {
                    Log.Instance.Write("Wallet import complete");
                    proc.StandardInput.WriteLine("exit");
                }
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();

            int exitCode = proc.ExitCode;

            Log.Instance.Write("Import wallet exited with code: {0}", exitCode);
        }
    }
}