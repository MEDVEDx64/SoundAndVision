using System;

namespace Massacre.Snv.Core.Network
{
    public class EndPointExceptionEventArgs : EventArgs
    {
        public Exception ExceptionObject { get; }
        public EndPointExceptionEventArgs(Exception e)
        {
            ExceptionObject = e;
        }
    }
}
