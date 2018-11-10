using Massacre.Snv.Core.Network.Utils;
using System;
using System.Collections.Generic;

namespace Massacre.Snv.Core.Network.Packets
{
    public class LoginPacket : PacketBase
    {
        public override int Number
        {
            get { return 100; }
        }

        public string Name { get; set; }
        public LoginMode Mode { get; set; }
        public int Version { get; set; }
        public Guid? Id { get; set; }

        public LoginPacket()
        {
            Mode = LoginMode.Unspecified;
        }

        protected override void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
            var q = ParseQuery(data[0]);
            Version = (int)q["PrV"];

            var mode = q["mode"].ToString().ToLower();
            if(mode == "client")
            {
                Mode = LoginMode.Client;
            }
            else if(mode == "admin")
            {
                Mode = LoginMode.Admin;
            }
            else
            {
                throw new PacketFormatException(rawPacket);
            }

            if(q.ContainsKey("name"))
            {
                Name = Base64.Decode((string)q["name"]);
            }

            if(q.ContainsKey("id"))
            {
                Id = (Guid)q["id"];
            }
        }

        public override string ToString()
        {
            var q = new Dictionary<string, object>();

            q["PrV"] = Version;
            if(Mode == LoginMode.Client)
            {
                q["mode"] = "client";
            }
            else if(Mode == LoginMode.Admin)
            {
                q["mode"] = "admin";
            }

            if(Name != null)
            {
                q["name"] = Base64.Encode(Name);
            }

            if(Id != null)
            {
                q["id"] = Id;
            }

            return ConstructPacketTier2(BuildQuery(q));
        }
    }
}
