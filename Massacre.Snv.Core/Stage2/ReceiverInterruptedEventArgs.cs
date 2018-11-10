using System;

namespace Massacre.Snv.Core.Stage2
{
    public class ReceiverInterruptedEventArgs : EventArgs
    {
        // When any of these comes non-zero, nterruption should be treated as error
        public int Code { get; }
        public Exception ExceptionObject { get; }

        public ReceiverInterruptedEventArgs(int code, Exception e = null)
        {
            Code = code;
            ExceptionObject = e;
        }
    }
}
