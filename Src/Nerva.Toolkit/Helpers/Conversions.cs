using System;
using AngryWasp.Logger;

namespace Nerva.Toolkit.Helpers
{	
	public static class Conversions
    {
        public static int OctetSetToInt(string vs)
        {
            int i = 0;

            string[] split = vs.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != 4)
            {
                Log.Instance.Write(Log_Severity.Error, "Attempt to convert octet set != 4 values");
                return -1;
            }

            int o1, o2, o3, o4;

            if (!int.TryParse(split[0], out o1) || !int.TryParse(split[1], out o2) || !int.TryParse(split[2], out o3) || !int.TryParse(split[3], out o4))
            {
                Log.Instance.Write(Log_Severity.Error, "Attempt to parse poorly formatted octet set");
                return -1;
            }

            if ((o1 < 0 || o1 > 255) || (o2 < 0 || o2 > 255) || (o3 < 0 || o3 > 255) || (o4 < 0 || o4 > 255))
            {
                Log.Instance.Write(Log_Severity.Error, "Octet value is out of range");
                return -1;
            }

            i += o1 * 16777216;
            i += o2 * 65536;
            i += o3 * 256;
            i += o4;

            return i;
        }

        public static DateTime UnixTimeStampToDateTime(ulong unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
            return dtDateTime;
        }
    }
}