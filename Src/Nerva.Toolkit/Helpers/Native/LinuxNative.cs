using AngryWasp.Logger;
using Mono.Unix.Native;

namespace Nerva.Toolkit.Helpers.Native
{
    public static class LinuxNative
    {
        public static void Chmod(string path, uint mode)
        {
            if(Syscall.chmod(path, (FilePermissions)mode) != 0)
                Log.Instance.Write(Log_Severity.Fatal, "Syscall 'chmod' failed.");
        }

        public static Utsname Uname()
        {
            Utsname results;

            if(Syscall.uname(out results) != 0)
            {
                Log.Instance.Write(Log_Severity.Fatal, "Syscall 'uname' failed.");
                return null;
            }

            return results;
        }

        public static void Symlink(string source, string target)
        {
            if(Syscall.symlink(source, target) != 0)
                Log.Instance.Write(Log_Severity.Warning, "Syscall 'symlink' failed. Possibly already exist");
        }
    }
}