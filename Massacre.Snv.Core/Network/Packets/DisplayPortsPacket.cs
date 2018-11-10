using System;
using System.Collections.Generic;
using System.Linq;

namespace Massacre.Snv.Core.Network.Packets
{
    public class DisplayPortsPacket : PacketBase
    {
        public override int Number
        {
            get { return 102; }
        }

        public List<ushort> Ports { get; }

        public DisplayPortsPacket()
        {
            Ports = new List<ushort>();
        }

        public DisplayPortsPacket(IEnumerable<ushort> ports)
        {
            Ports = ports.ToList();
        }

        protected override void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
            foreach (var p in data)
            {
                Ports.Add(Convert.ToUInt16(p));
            }
        }

        public override string ToString()
        {
            var pl = new List<object>();
            foreach(var p in Ports)
            {
                pl.Add(p);
            }

            return ConstructPacketTier1(pl);
        }
    }
}
