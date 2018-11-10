using System.Collections.Generic;

namespace Massacre.Snv.Core.Network.Packets
{
    public class AckPacket : PacketBase
    {
        public override int Number
        {
            get { return 202; }
        }

        protected override void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
        }

        public override string ToString()
        {
            return ConstructPacketTier2();
        }
    }
}
