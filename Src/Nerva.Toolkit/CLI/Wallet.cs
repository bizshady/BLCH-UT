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

            if (!BasicRequest("get_accounts", out result))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<Account>>(result).Result;
        }

        public bool StopWallet()
        {
            return BasicRequest("stop_wallet");
        }

        public bool CreateWallet(string walletName, string password)
        {
            string result = null;

            return BasicRequest<CreateWallet>("create_wallet", new CreateWallet {
                FileName = walletName,
                Password = password
            }, out result);
        }

        public bool OpenWallet(string walletName, string password)
        {
            string result = null;

            return BasicRequest<OpenWallet>("open_wallet", new OpenWallet {
                FileName = walletName,
                Password = password
            }, out result);
        }

        public string QueryKey(Key_Type keyType)
        {
            string result = null;

            if (!BasicRequest<QueryKey>("query_key", new QueryKey {
                KeyType = keyType.ToString().ToLower()
            }, out result))
                return null;

            return JObject.Parse(result)["result"]["key"].Value<string>();
        }

        public TransferList GetTransfers(uint scanFromHeight, out uint lastTxHeight)
        {
            string result = null;
            lastTxHeight = 0;

            if (!BasicRequest<GetTransfers>("get_transfers", new GetTransfers {
                ScanFromHeight = scanFromHeight
            }, out result))
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

        public bool RescanSpent()
        {
            return BasicRequest("rescan_spent");
        }

        public bool RescanBlockchain()
        {
            return BasicRequest("rescan_blockchain");
        }

        public bool Store()
        {
            return BasicRequest("store");
        }

        private bool BasicRequest(string rpc, bool suppressErrorMessage = false)
        {
            string result = null;
            return BasicRequest(rpc, out result, suppressErrorMessage);
        }

        private bool BasicRequest(string rpc, out string result, bool suppressErrorMessage = false)
        {
            result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = rpc
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result, suppressErrorMessage);
        }

        private bool BasicRequest<T>(string rpc, T param, out string result, bool suppressErrorMessage = false)
        {
            result = null;

            JsonRequest<T> jr = new JsonRequest<T>
            {
                MethodName = rpc,
                Params = param
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                return false;
            }

            return !CheckError(jr.MethodName, result, suppressErrorMessage);
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

            string cliPath = FileNames.GetCliExePath(FileNames.CLI_WALLET);
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
            if (Configuration.Instance.Testnet)
                args += " --testnet";

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