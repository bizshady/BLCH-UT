using System;
using System.Text;
using AngryWasp.Logger;

namespace Nerva.Toolkit.Helpers
{	
	public static class Conversions
    {
        public static string EncodeBase64(this string t)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(t));
        }

        public static string DecodeBase64(this string t)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(t));
        }


        public static double FromAtomicUnits(ulong i)
        {
            return ((double)i) / 1000000000000.0d;
        }

        public static string WalletAddressShortForm(string a)
        {
            return $"{a.Substring(0, 6)}...{a.Substring(a.Length - 6, 6)}";
        }

        public static ulong OctetSetToInt(string vs)
        {
            ulong i = 0;

            string[] split = vs.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != 4)
            {
                Log.Instance.Write(Log_Severity.Error, "Attempt to convert octet set != 4 values");
                return 0;
            }

            ulong o1, o2, o3, o4;

            if (!ulong.TryParse(split[0], out o1) || !ulong.TryParse(split[1], out o2) || !ulong.TryParse(split[2], out o3) || !ulong.TryParse(split[3], out o4))
            {
                Log.Instance.Write(Log_Severity.Error, "Attempt to parse poorly formatted octet set");
                return 0;
            }

            if ((o1 < 0 || o1 > 255) || (o2 < 0 || o2 > 255) || (o3 < 0 || o3 > 255) || (o4 < 0 || o4 > 255))
            {
                Log.Instance.Write(Log_Severity.Error, "Octet value is out of range");
                return 0;
            }

            i += o1 * 16777216;
            i += o2 * 65536;
            i += o3 * 256;
            i += o4;

            return i;
        }

        public static DateTime UnixTimeStampToDateTime(ulong ts)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(ts).ToLocalTime();
        }
    }
}