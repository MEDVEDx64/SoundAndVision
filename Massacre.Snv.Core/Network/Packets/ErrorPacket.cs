namespace Massacre.Snv.Core.Network.Packets
{
    public class ErrorPacket : TextPacket
    {
        public override int Number
        {
            get { return 204; }
        }
    }
}
