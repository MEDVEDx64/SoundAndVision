namespace Massacre.Snv.Core.Network.Packets
{
    public class PingPacket : TextPacket
    {
        public override int Number
        {
            get { return 200; }
        }
    }
}
