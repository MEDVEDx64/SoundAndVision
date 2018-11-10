using Massacre.Snv.Core.Network.Utils;
using System.Collections.Generic;

namespace Massacre.Snv.Core.Network.Packets
{
    public class DisplayInfoPacket : PacketBase
    {
        public override int Number
        {
            get { return 101; }
        }

        public IList<DisplaySize> Displays { get; }

        public DisplayInfoPacket()
        {
            Displays = new List<DisplaySize>();
        }

        protected override void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
            Displays.Clear();
            foreach(var ds in data)
            {
                Displays.Add(new DisplaySize(ds));
            }
        }

        public override string ToString()
        {
            return ConstructPacketTier1(Displays);
        }
    }
}
