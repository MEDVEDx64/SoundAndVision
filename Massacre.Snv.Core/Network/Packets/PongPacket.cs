namespace Massacre.Snv.Core.Network.Packets
{
    public class PongPacket : TextPacket
    {
        public override int Number
        {
            get { return 201; }
        }
    }
}
