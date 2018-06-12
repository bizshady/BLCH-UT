using System.IO;
using AngryWasp.Helpers;
using AngryWasp.Serializer;

namespace Nerva.Toolkit
{	
	public class Configuration
	{
        public string LogFile { get; set; } = null;


        public static Configuration New()
        {
            Configuration config = new Configuration();
            
            //TODO: Set default values for a new config file

            new ObjectSerializer().Serialize(config, Constants.DEFAULT_CONFIG_FILENAME);
            return config;
        }

        public static Configuration Load(string configFile = null)
        {
            //If no config file is specified, use App.conf
            //If the config file does not exist. Make a new one and return that

            if (configFile == null)
                configFile = Constants.DEFAULT_CONFIG_FILENAME;

            if (!File.Exists(configFile))
                return New();

            return new ObjectSerializer().Deserialize<Configuration>(XHelper.LoadDocument(configFile));
        }
    }
}