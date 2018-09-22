using System;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nerva.Rpc;
using Nerva.Rpc.Wallet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nerva.Toolkit.CLI
{
    public partial class WalletInterface : CliInterface
    {
        public GetAccountsResponseData GetAccounts()
        {
            GetAccountsResponseData data = null;

            new GetAccounts((GetAccountsResponseData result) => {
                data = result;
            }, null, r.Port).Run();

            return data;
        }

        public bool StopWallet()
        {
            return new StopWallet(null, null, r.Port).Run();
        }

        public bool CreateWallet(string walletName, string password)
        {
            return new CreateWallet(new CreateWalletRequestData {
                FileName = walletName,
                Password = password
            }, null, null, r.Port).Run();
        } 

        public bool OpenWallet(string walletName, string password)
        {
            return new OpenWallet(new OpenWalletRequestData {
                FileName = walletName,
                Password = password
            }, null, null, r.Port).Run();
        }

        public QueryKeyResponseData QueryKey(string keyType)
        {
            QueryKeyResponseData data = null;

            new QueryKey(new QueryKeyRequestData {
                KeyType = keyType
            }, (QueryKeyResponseData result) => {
                data = result;
            }, null, r.Port).Run();

            return data;
        }

        public GetTransfersResponseData GetTransfers(uint scanFromHeight, out uint lastTxHeight)
        {
            //todo: this only gets transfers for account index 0
            // need to get all transfers
            GetTransfersResponseData data = null;
            uint i = 0, o = 0, l = 0;
            lastTxHeight = 0;

            new GetTransfers(new GetTransfersRequestData {
                ScanFromHeight = scanFromHeight
            }, (GetTransfersResponseData result) =>
            {
                if (result.Incoming != null && result.Incoming.Count > 0)
                    i = result.Incoming[result.Incoming.Count - 1].Height;
                
                if (result.Outgoing != null && result.Outgoing.Count > 0)
                    o = result.Outgoing[result.Outgoing.Count - 1].Height;

                l = Math.Max(i, o);

                data = result;
            }, null, r.Port).Run();

            lastTxHeight = l;
            return data;
        }

        public bool RescanSpent()
        {
            return new RescanSpent(null, null, r.Port).Run();
        }

        public bool RescanBlockchain()
        {
            return new RescanBlockchain(null, null, r.Port).Run();
        }

        public bool Store()
        {
            return new Store(null, null, r.Port).Run();
        }

        public CreateAccountResponseData CreateAccount(string label)
        {
            CreateAccountResponseData data = null;

            new CreateAccount(new CreateAccountRequestData {
                Label = label
            }, (CreateAccountResponseData result) => {
                data = result;
            }, null, r.Port).Run();

            return data;
        }

        public bool LabelAccount(uint index, string label)
        {
            return new LabelAccount(new LabelAccountRequestData {
                Index = index,
                Label = label
            }, null, null, r.Port).Run();
        }

        public GetTransferByTxIDResponseData GetTransferByTxID(string txid)
        {
            GetTransferByTxIDResponseData data = null;

            new GetTransferByTxID(new GetTransferByTxIDRequestData {
                TxID = txid
            }, (GetTransferByTxIDResponseData result) => {
                data = result;
            }, null, r.Port).Run();

            return data;
        }

        public TransferResponseData TransferFunds(SubAddressAccount acc, string address, string paymentId, double amount, Send_Priority priority)
        {
            TransferResponseData data = null;

            new Transfer(new TransferRequestData {
                AccountIndex = acc.Index,
                Priority = (uint)priority,
                PaymentId = paymentId,
                Destinations = new List<TransferDestination> {
                    new TransferDestination {
                        Address = address,
                        Amount = Conversions.ToAtomicUnits(amount)
                    }
                }
            }, (TransferResponseData result) => {
                data = result;
            }, null, r.Port).Run();

            return data;
        }
    }
}