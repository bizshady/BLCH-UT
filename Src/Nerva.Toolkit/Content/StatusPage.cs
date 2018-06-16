using System;
using System.Collections.Generic;
using Nerva.Toolkit.CLI.Structures;
using Nerva.Toolkit.Helpers;
using System.Linq;
using AngryWasp.Logger;

namespace Nerva.Toolkit.Content
{
    public partial class StatusPage
	{
        List<string> la = new List<string>();
        public void Update(Info info, List<Connection> connections, MiningStatus mStatus)
        {
            if (info != null)
            {
                double nethash = Math.Round(((info.Difficulty / 60.0d) / 1000.0d), 2);
                //Update the daemon info
                lblHeight.Text = info.Height.ToString();
                lblNetHash.Text = nethash.ToString() + " kH/s";
                lblRunTime.Text = (DateTime.Now - Conversions.UnixTimeStampToDateTime((ulong)info.StartTime)).ToString(@"hh\:mm\:ss");

                if (info.Mainnet)
					lblNetwork.Text = "MainNet";
                else if (info.Testnet)
                    lblNetwork.Text = "TestNet";
                else
                    Log.Instance.Write(Log_Severity.Fatal, "Unknown network connection type");
            }
            else
            {
                lblHeight.Text = "-";
                lblNetHash.Text = "-";
                lblRunTime.Text = "-";
            }

            if (mStatus != null && mStatus.Active)
            {
                lblMinerStatus.Text = "Miner (Active)";
                lblMiningAddress.Text = mStatus.Address.Substring(0, 6) + "..." + mStatus.Address.Substring(mStatus.Address.Length - 6, 6);
                lblMiningThreads.Text = mStatus.ThreadCount.ToString();

                string speed;
                if (mStatus.Speed > 1000)
                    speed = $"{mStatus.Speed / 1000.0d} kH/s";
                else
                    speed = $"{(double)mStatus.Speed} h/s";
                
                lblMiningHashrate.Text = speed;
            }
            else
            {
                lblMinerStatus.Text = "Miner (Inactive)";
                lblMiningAddress.Text = "-";
                lblMiningThreads.Text = "-";
                lblMiningHashrate.Text = "-";
            }

            if (connections != null)
            {
                //Check if we need to update the connections list
                List<string> a = connections.Select(x => x.Address).ToList();

                bool needUpdate = false;
                if (a.Count != la.Count)
                    needUpdate = true;

                if (!needUpdate)
                    for (int i = 0; i < a.Count; i++)
                        if (a[i] != la[i])
                        {
                            needUpdate = true;
                            break;
                        }

                if (needUpdate)
                {
                    mainControl.SuspendLayout();
                    CreateConnectionsTable(connections);
                    mainControl.ResumeLayout();
                    la = a;
                }
            }
            else
            {
                connectionsContainer.Content = null;
            }
        }
    }
}