using Massacre.Snv.Core.Network.Utils;
using System.Collections.Generic;

namespace Massacre.Snv.Core.Network.Packets
{
    public class NotificationPacket : PacketBase
    {
        public override int Number
        {
            get { return 103; }
        }

        public string Text { get; set; }
        public bool SoundAlert { get; set; }

        protected override void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
            if(data.Count > 0)
            {
                var q = ParseQuery(data[0]);
                if(q.ContainsKey("text"))
                {
                    Text = Base64.Decode(q["text"].ToString());
                }

                if(q.ContainsKey("beep"))
                {
                    SoundAlert = (bool)q["beep"];
                }
            }
        }

        public override string ToString()
        {
            var q = new Dictionary<string, object>();
            q.Add("text", Base64.Encode(Text));
            if(SoundAlert)
            {
                q.Add("beep", SoundAlert);
            }

            return ConstructPacketTier2(BuildQuery(q));
        }
    }
}
