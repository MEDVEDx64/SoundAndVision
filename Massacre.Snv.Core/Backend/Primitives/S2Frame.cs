using System;

namespace Massacre.Snv.Core.Backend.Primitives
{
    public struct S2Frame
    {
        public ushort Width;
        public ushort Height;
        public IntPtr Pixels;

        public int Error;
        public int Flags;
    }
}
