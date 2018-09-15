using System.Runtime.InteropServices;

namespace Nerva.Toolkit.Helpers.Native
{
    public static class LinuxNative
    {
		  [DllImport ("libc", SetLastError=true, EntryPoint="chmod")]
		  public static extern int sys_chmod (string path, uint mode);
    }
}