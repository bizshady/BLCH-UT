using System;
using AngryWasp.Logger;

#if (UNIX)
    using Mono.Unix.Native;
#endif

namespace Nerva.Toolkit.Helpers.Native
{
    public static class UnixNative
    {
        public static void Chmod(string path, uint mode)
        {
#if (UNIX)
            if(Syscall.chmod(path, (FilePermissions)mode) != 0)
                Log.Instance.Write(Log_Severity.Fatal, "Syscall 'chmod' failed.");
#else
            Log.Instance.Write(Log_Severity.Fatal, "Unix native functions are not supported in this configuration");
#endif
        }

        public static string Sysname()
        {
#if (UNIX)
            Utsname results;

            if(Syscall.uname(out results) != 0)
            {
                Log.Instance.Write(Log_Severity.Fatal, "Syscall 'uname' failed.");
                return null;
            }

            return results.sysname.ToLower();
#else
            Log.Instance.Write(Log_Severity.Fatal, "Unix native functions are not supported in this configuration");
            return null;
#endif
        }

        public static void Symlink(string source, string target)
        {
#if (UNIX)
            if(Syscall.symlink(source, target) != 0)
                Log.Instance.Write(Log_Severity.Warning, "Syscall 'symlink' failed. Possibly already exist");
#else
            Log.Instance.Write(Log_Severity.Fatal, "Unix native functions are not supported in this configuration");
#endif
        }
    }
}