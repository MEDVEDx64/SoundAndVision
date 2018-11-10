using System;

namespace Massacre.Snv.Core.Backend.Primitives
{
    public struct RecorderContext
    {

#pragma warning disable 0169

        int IsRecording;

        IntPtr CodecContext;
        IntPtr FormatContext;
        ulong BytesWritten;
        uint ChunksWritten;

        ulong Pootis;
        ulong Sync;

#pragma warning restore 0169

    }
}
