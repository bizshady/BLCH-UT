using System;
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

    public partial class WalletInterface : CliInterface
    {
        public class RpcWalletError
        {
            public bool SupressLogging { get; set; } = true;

            public int Code { get; set; }
            public string Message { get; set; }
        }

        public Account GetAccounts()
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();

            if (!BasicRequest("get_accounts", out result, ref e))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<Account>>(result).Result;
        }

        public bool StopWallet()
        {
            RpcWalletError e = new RpcWalletError();
            return BasicRequest("stop_wallet", ref e);
        }

        public bool CreateWallet(string walletName, string password)
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();

            return BasicRequest<CreateWallet>("create_wallet", new CreateWallet {
                FileName = walletName,
                Password = password
            }, out result, ref e);
        }

        public bool OpenWallet(string walletName, string password)
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();

            var request = BasicRequest<OpenWallet>("open_wallet", new OpenWallet {
                FileName = walletName,
                Password = password
            }, out result, ref e);

            return request;
        }

        public KeyInfo QueryKey(Key_Type keyType)
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();

            if (!BasicRequest<QueryKey>("query_key", new QueryKey {
                KeyType = keyType.ToString().ToLower()
            }, out result, ref e))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<KeyInfo>>(result).Result;
        }

        public TransferList GetTransfers(uint scanFromHeight, out uint lastTxHeight)
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();
            lastTxHeight = 0;

            if (!BasicRequest<GetTransfers>("get_transfers", new GetTransfers {
                ScanFromHeight = scanFromHeight
            }, out result, ref e))
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
            RpcWalletError e = new RpcWalletError();
            return BasicRequest("rescan_spent", ref e);
        }

        public bool RescanBlockchain()
        {
            RpcWalletError e = new RpcWalletError();
            return BasicRequest("rescan_blockchain", ref e);
        }

        public bool Store()
        {
            RpcWalletError e = new RpcWalletError();
            return BasicRequest("store", ref e);
        }

        public NewAccount CreateAccount(string label)
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();

            if (string.IsNullOrEmpty(label))
            {
                if (!BasicRequest("create_account", out result, ref e))
                    return null;
            }
            else
            {
                if (!BasicRequest<CreateAccount>("create_account", new CreateAccount {
                    Label = label
                }, out result, ref e))
                    return null;
            }

            return JsonConvert.DeserializeObject<JsonValue<NewAccount>>(result).Result;
        }

        public bool LabelAccount(uint index, string label)
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();

            return BasicRequest<LabelAccount>("label_account", new LabelAccount {
                Index = index,
                Label = label
            }, out result, ref e);
        }

        public TransferTxID GetTransferByTxID(string txid)
        {
            string result = null;
            RpcWalletError e = new RpcWalletError();

            if (!BasicRequest<GetTransferByTxID>("get_transfer_by_txid", new GetTransferByTxID {
                TxID = txid
            }, out result, ref e))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<TransferContainer>>(result).Result.Transfer;
        }

        public Send TransferFunds(SubAddressAccount acc, string address, string paymentId, double amount, Send_Priority priority, ref RpcWalletError e)
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
                }, ref e, dest);
            }
            else
            {
                sendResponse = TransferFunds<SendWithPaymentID>(new SendWithPaymentID {
                    AccountIndex = acc.Index,
                    Priority = (uint)priority,
                    PaymentId = paymentId
                }, ref e, dest);
            }

            return sendResponse;
        }

        public Send TransferFunds<T>(T sendData, ref RpcWalletError e, params Destination[] destinations) where T : SendWithoutPaymentID, new()
        {
            string result = null;

            sendData.Destinations.AddRange(destinations);

            if (!BasicRequest<T>("transfer", sendData, out result, ref e))
                return null;

            return JsonConvert.DeserializeObject<JsonValue<Send>>(result).Result;
        }

        private bool BasicRequest(string rpc, ref RpcWalletError e)
        {
            string result = null;
            return BasicRequest(rpc, out result, ref e);
        }

        private bool BasicRequest(string rpc, out string result, ref RpcWalletError e)
        {
            result = null;

            JsonRequest jr = new JsonRequest
            {
                MethodName = rpc
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);

                return false;
            }

            return !CheckError(jr.MethodName, result, ref e);
        }

        private bool BasicRequest<T>(string rpc, T param, out string result, ref RpcWalletError e)
        {
            result = null;

            JsonRequest<T> jr = new JsonRequest<T>
            {
                MethodName = rpc,
                Params = param
            };

            if (!netHelper.MakeJsonRpcRequest(jr, out result))
            {
                if (Configuration.Instance.LogRpcErrors)
                    Log.Instance.Write(Log_Severity.Error, "Could not complete JSON RPC call: {0}", jr.MethodName);
                    
                return false;
            }

            return !CheckError(jr.MethodName, result, ref e);
        }

        private bool CheckError(string methodName, string result, ref RpcWalletError e)
        {
            var error = JObject.Parse(result)["error"];

            if (error != null)
            {
                int code = error["code"].Value<int>();
                string message = error["message"].Value<string>();

                e.Code = code;
                e.Message = message;

                if (!e.SupressLogging)
                    Log.Instance.Write(Log_Severity.Error, "Wallet.{0}: Code {1}, Message: '{2}'", methodName, code, message);
                return true;
            }

            return false;
        }
    }
}