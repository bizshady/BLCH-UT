using System;
using Nerva.Toolkit.Helpers.Native;

namespace Nerva.Toolkit.Helpers
{	
	public static class Constants
	{
        public const string VERSION = "0.0.2.0";
        public const string CODE_NAME = "Beta 1";
        public static readonly string LONG_VERSION = $"{VERSION}: {CODE_NAME}";

        public const string DEFAULT_CONFIG_FILENAME = "app.config";
        public const string DEFAULT_LOG_FILENAME = "app.log";
        public const string DEV_WALLET_ADDRESS = "NV1r8P6THPASAQX77re6hXTMJ1ykXXvtYXFXgMv4vFAQNYo3YatUvZ8LFNRu4dPQBjTwqJbMvqoeiipywmREPHpD2AgWnmG7Q";
        public const int ONE_SECOND = 1000;
        public const int FIVE_SECONDS = 5000;
        public const int BAN_TIME = 6000;

        public static readonly string[] Languages = new string[]
        {
            "Deutsch",
            "English",
            "Español",
            "Français",
            "Italiano",
            "Nederlands",
            "Português",
            "русский язык",
            "日本語",
            "简体中文 (中国)",
            "Esperanto",
            "Lojban"
        };
    }

    public enum OS_Type
    {
        Linux,
        Mac,
        Windows,
        Unsupported,
    }

    public static class OS
    {
        public static OS_Type Type
        {
            get
            {
                var p = Environment.OSVersion.Platform;

                switch (p)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                        return OS_Type.Windows;
                    case PlatformID.Unix:
                    {
                        var uname = LinuxNative.Uname().sysname.ToLower();
                        if (uname == "linux")
                            return OS_Type.Linux;
                        else if (uname == "darwin")
                            return OS_Type.Mac;
                        else
                            return OS_Type.Unsupported; 
                    }     
                    default:
                        return OS_Type.Unsupported; 
                }
            }
        }
    }

    public static class SeedNodes
    {
        public const string XNV1 = "206.81.2.15";
        public const string XNV2 = "206.81.2.16";
        public const string XNV3 = "206.81.12.28";
        public const string XNV4 = "204.48.17.173";
        public const string XNV5 = "206.81.2.10";
        public const string XNV6 = "206.81.2.12";
    }
}