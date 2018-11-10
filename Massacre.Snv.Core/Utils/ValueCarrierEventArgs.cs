using System;

namespace Massacre.Snv.Core.Utils
{
    public class ValueCarrierEventArgs<T> : EventArgs
    {
        public T Value { get; }

        public ValueCarrierEventArgs(T value)
        {
            Value = value;
        }
    }
}
