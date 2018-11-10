namespace Massacre.Snv.Core.Network.Packets
{
    public class DataIgnoredPacket : AckPacket
    {
        public override int Number
        {
            get { return 203; }
        }
    }
}
