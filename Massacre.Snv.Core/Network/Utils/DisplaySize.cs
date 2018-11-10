using System;

namespace Massacre.Snv.Core.Network.Utils
{
    public class DisplaySize
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }

        public DisplaySize(ushort w = 0, ushort h = 0)
        {
            Width = w;
            Height = h;
        }

        public DisplaySize(string s) // SIC! - intended to throw exceptions if any
        {
            var sp = s.Split('x');
            Width = Convert.ToUInt16(sp[0]);
            Height = Convert.ToUInt16(sp[1]);
        }

        public override string ToString()
        {
            return Width + "x" + Height;
        }
    }
}
