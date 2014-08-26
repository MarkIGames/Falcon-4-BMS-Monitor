using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace F4toA3Monitor
{
    public class WindowsAPI
    {

        private WindowsAPI() { }

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);


        public static bool Peek(System.Diagnostics.Process proc, int target, byte[] data)
        {
            return ReadProcessMemory(proc.Handle, new IntPtr(target), data, new UIntPtr((uint)data.Length), new IntPtr(0));
        }

        public static bool Poke(System.Diagnostics.Process proc, int target, byte[] data)
        {
            IntPtr bytesWritten = new IntPtr(0);
            return WriteProcessMemory(proc.Handle, new IntPtr(target), data, new UIntPtr((uint)data.Length), out bytesWritten);
        }

    }
}
