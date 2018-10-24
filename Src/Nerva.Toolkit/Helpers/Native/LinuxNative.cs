using System;
using System.Runtime.InteropServices;
using AngryWasp.Logger;
using Mono.Posix;
using Mono.Unix.Native;

namespace Nerva.Toolkit.Helpers.Native
{
    public static class LinuxNative
    {
        public static void Chmod(string path, uint mode)
        {
            Mono.Unix.Native.Syscall.chmod(path, (FilePermissions)mode);
        }

        public static Utsname Uname()
        {
            Utsname results;

            if(Mono.Unix.Native.Syscall.uname(out results) != 0)
            {
                Log.Instance.Write(Log_Severity.Fatal, "Syscall 'uname' failed.");
                return null;
            }

            return results;
        }
    }
}