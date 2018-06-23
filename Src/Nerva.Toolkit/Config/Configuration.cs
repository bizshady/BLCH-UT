using System.IO;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Config
{	
	public class Configuration
	{
        #region Configuration properties

        public string ToolsPath { get; set; }

        public bool CheckForUpdateOnStartup { get; set; }

        public bool LogRpcTraffic { get; set; }

        public bool Testnet { get; set; }
        
        public Daemon Daemon { get; set; }

        public Wallet Wallet { get; set; }

        public bool ReconnectToDaemonProcess { get; set; }

        #endregion

        #region Not serialized
        
        [SerializerExclude]
        public bool NewDaemonOnStartup { get; set; } = true;

        #endregion

        public static Configuration New()
        {
            return new Configuration
            {
                ToolsPath = "./",
                CheckForUpdateOnStartup = true,
                LogRpcTraffic = false,
                Testnet = false,
                Daemon = Daemon.New(),
                Wallet = Wallet.New(),
                ReconnectToDaemonProcess = true
            };
        }

        #region Integration code

        private static string loadedConfigFile;
        private static Configuration instance;

        public static string LoadedConfigFile => loadedConfigFile;

        public static Configuration Instance => instance;

        public static void Load(string file)
        {
            loadedConfigFile = file;

            if (!File.Exists(loadedConfigFile))
            {
                instance = Configuration.New();
            
                //TODO: Set default values for a new config file
                
                Log.Instance.Write("Configuration created at '{0}'", loadedConfigFile);
                new ObjectSerializer().Serialize(instance, loadedConfigFile);
            }
            else
            {
                Log.Instance.Write("Configuration loaded from '{0}'", loadedConfigFile);
                var os = new ObjectSerializer();
                instance = os.Deserialize<Configuration>(XHelper.LoadDocument(loadedConfigFile));
            }
        }

        public static void Save()
        {
            new ObjectSerializer().Serialize(instance, loadedConfigFile);
            Log.Instance.Write("Configuration saved to '{0}'", loadedConfigFile);
        }

        #endregion
    }
}