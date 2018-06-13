using System;

namespace Nerva.Toolkit.Helpers
{	
	public static class HardwareHelper
	{
        public static int GetOptimalMiningThreadCount()
        {
            //TODO: Need a cross platform way to get the number of physical CPU cores on the PC
            return Environment.ProcessorCount / 2;
        }
    }
}