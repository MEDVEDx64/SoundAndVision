using Massacre.Snv.Core.Network.Utils;
using System.Collections.Generic;

namespace Massacre.Snv.Core.Network.Packets
{
    public abstract class TextPacket : PacketBase
    {
        public string Text { get; set; }

        protected override void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
            if(data.Count > 0)
            {
                Text = Base64.Decode(data[0]);
            }
        }

        public override string ToString()
        {
            if(Text != null)
            {
                return ConstructPacketTier2(Base64.Encode(Text));
            }

            return ConstructPacketTier2();
        }
    }
}
