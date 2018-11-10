using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Massacre.Snv.Core.Backend
{
    struct Win32Point
    {
        public int X;
        public int Y;
    }

    public static class UnmanagedTools
    {
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr SetMemory(IntPtr dest, int v, int count);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(ref Win32Point point);

        public static Point GetAbsoluteMousePosition()
        {
            var point = new Win32Point();
            GetCursorPos(ref point);
            return new Point(point.X, point.Y);
        }
    }
}
