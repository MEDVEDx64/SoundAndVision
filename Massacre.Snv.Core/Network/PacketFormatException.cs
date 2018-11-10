using System;

namespace Massacre.Snv.Core.Network
{
    public class PacketFormatException : Exception
    {
        public string Packet { get; }

        public override string Message
        {
            get { return "Malformed packet"; }
        }

        public PacketFormatException(string packet)
        {
            Packet = packet;
        }
    }
}
