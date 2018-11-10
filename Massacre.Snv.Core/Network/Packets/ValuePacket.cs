using System.Collections.Generic;

namespace Massacre.Snv.Core.Network.Packets
{
    public class ValuePacket : PacketBase
    {
        public IDictionary<string, object> Data { get; private set; }

        public override int Number
        {
            get { return 300; }
        }

        public ValuePacket()
        {
            Data = new Dictionary<string, object>();
        }

        protected override void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
            Data = ParseQuery(data[0]);
        }

        public override string ToString()
        {
            return ConstructPacketTier2(BuildQuery(Data));
        }
    }
}
