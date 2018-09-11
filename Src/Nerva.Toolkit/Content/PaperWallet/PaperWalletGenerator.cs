using System;
using AngryWasp.Helpers;

namespace Nerva.Toolkit.Content.PaperWallet
{	
	public static class PaperWalletGenerator
	{
        public static void GenerateWallet(string language)
        {

        }

        public static void Random32(UInt32 bits)
        {
            string seed = StringHelper.GenerateRandomHexString(32, true);

        }

        unsafe private static void sc_reduce32(char* s)
        {
            ulong s0 = 2097151 & load_3(s);
            ulong s1 = 2097151 & (load_4(s + 2) >> 5);
            ulong s2 = 2097151 & (load_3(s + 5) >> 2);
            ulong s3 = 2097151 & (load_4(s + 7) >> 7);
            ulong s4 = 2097151 & (load_4(s + 10) >> 4);
            ulong s5 = 2097151 & (load_3(s + 13) >> 1);
            ulong s6 = 2097151 & (load_4(s + 15) >> 6);
            ulong s7 = 2097151 & (load_3(s + 18) >> 3);
            ulong s8 = 2097151 & load_3(s + 21);
            ulong s9 = 2097151 & (load_4(s + 23) >> 5);
            ulong s10 = 2097151 & (load_3(s + 26) >> 2);
            ulong s11 = (load_4(s + 28) >> 7);
            ulong s12 = 0;

            ulong carry0 = (s0 + (1<<20)) >> 21; s1 += carry0; s0 -= carry0 << 21;
            ulong carry2 = (s2 + (1<<20)) >> 21; s3 += carry2; s2 -= carry2 << 21;
            ulong carry4 = (s4 + (1<<20)) >> 21; s5 += carry4; s4 -= carry4 << 21;
            ulong carry6 = (s6 + (1<<20)) >> 21; s7 += carry6; s6 -= carry6 << 21;
            ulong carry8 = (s8 + (1<<20)) >> 21; s9 += carry8; s8 -= carry8 << 21;
            ulong carry10 = (s10 + (1<<20)) >> 21; s11 += carry10; s10 -= carry10 << 21;

            ulong carry1 = (s1 + (1<<20)) >> 21; s2 += carry1; s1 -= carry1 << 21;
            ulong carry3 = (s3 + (1<<20)) >> 21; s4 += carry3; s3 -= carry3 << 21;
            ulong carry5 = (s5 + (1<<20)) >> 21; s6 += carry5; s5 -= carry5 << 21;
            ulong carry7 = (s7 + (1<<20)) >> 21; s8 += carry7; s7 -= carry7 << 21;
            ulong carry9 = (s9 + (1<<20)) >> 21; s10 += carry9; s9 -= carry9 << 21;
            ulong carry11 = (s11 + (1<<20)) >> 21; s12 += carry11; s11 -= carry11 << 21;

            s0 += s12 * 666643;
            s1 += s12 * 470296;
            s2 += s12 * 654183;
            s3 -= s12 * 997805;
            s4 += s12 * 136657;
            s5 -= s12 * 683901;
            s12 = 0;

            carry0 = s0 >> 21; s1 += carry0; s0 -= carry0 << 21;
            carry1 = s1 >> 21; s2 += carry1; s1 -= carry1 << 21;
            carry2 = s2 >> 21; s3 += carry2; s2 -= carry2 << 21;
            carry3 = s3 >> 21; s4 += carry3; s3 -= carry3 << 21;
            carry4 = s4 >> 21; s5 += carry4; s4 -= carry4 << 21;
            carry5 = s5 >> 21; s6 += carry5; s5 -= carry5 << 21;
            carry6 = s6 >> 21; s7 += carry6; s6 -= carry6 << 21;
            carry7 = s7 >> 21; s8 += carry7; s7 -= carry7 << 21;
            carry8 = s8 >> 21; s9 += carry8; s8 -= carry8 << 21;
            carry9 = s9 >> 21; s10 += carry9; s9 -= carry9 << 21;
            carry10 = s10 >> 21; s11 += carry10; s10 -= carry10 << 21;
            carry11 = s11 >> 21; s12 += carry11; s11 -= carry11 << 21;

            s0 += s12 * 666643;
            s1 += s12 * 470296;
            s2 += s12 * 654183;
            s3 -= s12 * 997805;
            s4 += s12 * 136657;
            s5 -= s12 * 683901;

            carry0 = s0 >> 21; s1 += carry0; s0 -= carry0 << 21;
            carry1 = s1 >> 21; s2 += carry1; s1 -= carry1 << 21;
            carry2 = s2 >> 21; s3 += carry2; s2 -= carry2 << 21;
            carry3 = s3 >> 21; s4 += carry3; s3 -= carry3 << 21;
            carry4 = s4 >> 21; s5 += carry4; s4 -= carry4 << 21;
            carry5 = s5 >> 21; s6 += carry5; s5 -= carry5 << 21;
            carry6 = s6 >> 21; s7 += carry6; s6 -= carry6 << 21;
            carry7 = s7 >> 21; s8 += carry7; s7 -= carry7 << 21;
            carry8 = s8 >> 21; s9 += carry8; s8 -= carry8 << 21;
            carry9 = s9 >> 21; s10 += carry9; s9 -= carry9 << 21;
            carry10 = s10 >> 21; s11 += carry10; s10 -= carry10 << 21;

            s[0] = (char)(s0 >> 0);
            s[1] = (char)(s0 >> 8);
            s[2] = (char)((s0 >> 16) | (s1 << 5));
            s[3] = (char)(s1 >> 3);
            s[4] = (char)(s1 >> 11);
            s[5] = (char)((s1 >> 19) | (s2 << 2));
            s[6] = (char)(s2 >> 6);
            s[7] = (char)((s2 >> 14) | (s3 << 7));
            s[8] = (char)(s3 >> 1);
            s[9] = (char)(s3 >> 9);
            s[10] = (char)((s3 >> 17) | (s4 << 4));
            s[11] = (char)(s4 >> 4);
            s[12] = (char)(s4 >> 12);
            s[13] = (char)((s4 >> 20) | (s5 << 1));
            s[14] = (char)(s5 >> 7);
            s[15] = (char)((s5 >> 15) | (s6 << 6));
            s[16] = (char)(s6 >> 2);
            s[17] = (char)(s6 >> 10);
            s[18] = (char)((s6 >> 18) | (s7 << 3));
            s[19] = (char)(s7 >> 5);
            s[20] = (char)(s7 >> 13);
            s[21] = (char)(s8 >> 0);
            s[22] = (char)(s8 >> 8);
            s[23] = (char)((s8 >> 16) | (s9 << 5));
            s[24] = (char)(s9 >> 3);
            s[25] = (char)(s9 >> 11);
            s[26] = (char)((s9 >> 19) | (s10 << 2));
            s[27] = (char)(s10 >> 6);
            s[28] = (char)((s10 >> 14) | (s11 << 7));
            s[29] = (char)(s11 >> 1);
            s[30] = (char)(s11 >> 9);
            s[31] = (char)(s11 >> 17);
        }

        unsafe private static ulong load_3(char* i)
        {
            ulong result;
            result = (ulong) i[0];
            result |= ((ulong) i[1]) << 8;
            result |= ((ulong) i[2]) << 16;
            return result;
        }

        unsafe private static ulong load_4(char* i)
        {
            ulong result;
            result = (ulong) i[0];
            result |= ((ulong) i[1]) << 8;
            result |= ((ulong) i[2]) << 16;
            result |= ((ulong) i[3]) << 24;
            return result;
        }
    }
}