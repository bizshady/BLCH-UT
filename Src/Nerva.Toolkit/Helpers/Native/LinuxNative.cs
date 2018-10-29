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
            if(Mono.Unix.Native.Syscall.chmod(path, (FilePermissions)mode) != 0)
                Log.Instance.Write(Log_Severity.Fatal, "Syscall 'chmod' failed.");
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

        public static void Symlink(string source, string target)
        {
            if(Mono.Unix.Native.Syscall.symlink(source, target) != 0)
                Log.Instance.Write(Log_Severity.Warning, "Syscall 'symlink' failed. Possibly already exist");
        }
    }
}