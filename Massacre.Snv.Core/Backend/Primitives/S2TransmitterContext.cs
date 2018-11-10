using System;

namespace Massacre.Snv.Core.Backend.Primitives
{
    public struct S2TransmitterContext
    {
        public ulong Counter;

#pragma warning disable 0169
        
        int Socket;
        IntPtr PreviousPicture;

        ushort PreviousWidth;
        ushort PreviousHeight;

#pragma warning restore 0169

    }
}
