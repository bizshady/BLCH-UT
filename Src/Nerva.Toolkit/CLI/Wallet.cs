using System;
using System.Collections.Generic;
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
        Secret_View_Key,
        Secret_Spend_Key,
        Public_View_Key,
        Public_Spend_Key,
        All_Keys,
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

        public KeyInfo QueryKey(Key_Type keyType)
        {
            string result = null;

            if (!BasicRequest<QueryKey>("query_key", new QueryKey {
                KeyType = keyType.ToString().ToLower()
            }, out result))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<KeyInfo>>(result).Result;
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

        public NewAccount CreateAccount(string label)
        {
            string result = null;

            if (string.IsNullOrEmpty(label))
            {
                if (!BasicRequest("create_account", out result))
                    return null;
            }
            else
            {
                if (!BasicRequest<CreateAccount>("create_account", new CreateAccount {
                    Label = label
                }, out result))
                    return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<NewAccount>>(result).Result;
        }

        public bool LabelAccount(uint index, string label)
        {
            string result = null;

            return BasicRequest<LabelAccount>("label_account", new LabelAccount {
                Index = index,
                Label = label
            }, out result);
        }

        public TransferTxID GetTransferByTxID(string txid)
        {
            string result = null;

            if (!BasicRequest<GetTransferByTxID>("get_transfer_by_txid", new GetTransferByTxID {
                TxID = txid
            }, out result))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<TransferContainer>>(result).Result.Transfer;
        }

        public Send TransferFunds(SubAddressAccount acc, string address, string paymentId, double amount, Send_Priority priority)
        {
            var dest = new Destination
            {
                Address = address,
                Amount = Conversions.ToAtomicUnits(amount)
            };

            Send sendResponse = null;

            if (string.IsNullOrEmpty(paymentId))
            {
                sendResponse = TransferFunds<SendWithoutPaymentID>(new SendWithoutPaymentID {
                    AccountIndex = acc.Index,
                    Priority = (uint)priority
                }, dest);
            }
            else
            {
                sendResponse = TransferFunds<SendWithPaymentID>(new SendWithPaymentID {
                    AccountIndex = acc.Index,
                    Priority = (uint)priority,
                    PaymentId = paymentId
                }, dest);
            }

            return sendResponse;
        }

        public Send TransferFunds<T>(T sendData, params Destination[] destinations) where T : SendWithoutPaymentID, new()
        {
            string result = null;

            sendData.Destinations.AddRange(destinations);

            if (!BasicRequest<T>("transfer", sendData, out result))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<Send>>(result).Result;
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

            [JsonProperty("create_address_file")]
            public bool CreateAddressFile => true;
        }

        [JsonObject]
        private class RestoreJsonKeys : RestoreJson
        {
            [JsonProperty("viewkey")]
            public string ViewKey { get; set; } = string.Empty;

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

        public void RestoreWalletFromKeys(string walletName, string viewKey, string spendKey, string pass)
        {
            string walletPath = Path.Combine(Configuration.Instance.Wallet.WalletDir, walletName);

            CreateJson(walletPath, new RestoreJsonKeys
            {
                FileName = walletPath,
                Password = pass,
                ViewKey = viewKey,
                SpendKey = spendKey
            });

            RestoreWalletFromJson(walletPath);
        }

        public void RestoreWalletFromSeed(string walletName, string seed, string pass)
        {
            string walletPath = Path.Combine(Configuration.Instance.Wallet.WalletDir, walletName);

            CreateJson(walletPath, new RestoreJsonSeed
            {
                FileName = walletPath,
                Password = pass,
                Seed = seed,
            });

            RestoreWalletFromJson(walletPath);
        }
        
        private void CreateJson(string walletPath, object data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            var serializer = JsonSerializer.Create(settings);

            using (var sw = new StreamWriter(File.OpenWrite($"{walletPath}.json")))
                using (var jsonTextWriter = new JsonTextWriter(sw))
                    serializer.Serialize(jsonTextWriter, data);
        }

        private void RestoreWalletFromJson(string walletPath)
        {
            string daemon = $"127.0.0.1:{Configuration.Instance.Daemon.Rpc.Port}";
            string exe = FileNames.GetCliExePath(FileNames.CLI_WALLET);
            string args = $"--daemon-address {daemon} --generate-from-json {walletPath}.json";
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