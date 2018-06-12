using AngryWasp.Serializer;

namespace Nerva.Toolkit
{	
	public class Configuration
	{
        public string LogFile { get; set; } = null


        public static void New()
        {

        }

        public static void Load()
        {
            Configuration config = new Configuration();
            new ObjectSerializer().
        }
    }
}