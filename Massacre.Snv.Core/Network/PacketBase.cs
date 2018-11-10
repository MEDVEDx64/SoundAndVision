using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Massacre.Snv.Core.Network
{
    // 📣 Attention! All the packet developers!
    // There are three things you should care about: Number, ParsePacket(data, rawPacket), ToString().
    // Read the comments below.

    public abstract class PacketBase
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        public static readonly char Delimiter = '\n';
        public static readonly int ProtocolVersion = 3;

        // The unique packet number. Go define it.
        public virtual int Number
        {
            get { throw new NotImplementedException(); }
        }

        // Returns true on success, false on packet type mismatch.
        // May throw exceptions when facing to malformed packets.
        public bool ParsePacket(string packet)
        {
            var data = InspectPacket(packet);
            if (Convert.ToInt32(data[0]) != Number)
            {
                return false;
            }

            var crop = data.ToList();
            crop.RemoveAt(0);
            ParsePacket(crop, packet);
            return true;
        }

        // Packet parsing done here.
        // 'data' represents packet payload arguments WITHOUT leading number field.
        protected virtual void ParsePacket(IReadOnlyList<string> data, string rawPacket)
        {
            throw new NotImplementedException();
        }

        // Used to convert the object back to raw packet string.
        // Use ConstructPacket() to produce final output.
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        protected string ConstructPacketTier1(IEnumerable<object> args)
        {
            var packet = new StringBuilder();
            packet.Append(Number);

            foreach (var x in args)
            {
                packet.Append(" ");
                packet.Append(x);
            }

            packet.Append(Delimiter);
            return packet.ToString();
        }

        protected string ConstructPacketTier2(params object[] args)
        {
            return ConstructPacketTier1(args);
        }

        static IReadOnlyList<string> InspectPacket(string packet)
        {
            var args = packet.Replace(Delimiter, ' ').Trim().Split(' ');
            int probe;
            if(args.Length < 1 || !int.TryParse(args[0], out probe))
            {
                throw new PacketFormatException(packet);
            }

            return args;
        }

        protected static string BuildQuery(IDictionary<string, object> data)
        {
            var query = new StringBuilder();
            foreach(var k in data.Keys)
            {
                if(query.Length > 0)
                {
                    query.Append("&");
                }
                query.Append(k + "=" + data[k]);
            }

            return query.ToString();
        }

        protected static IDictionary<string, object> ParseQuery(string query)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach(var entry in query.Split('&'))
            {
                var kv = entry.Split('=');

                int i;
                if(int.TryParse(kv[1], out i))
                {
                    data[kv[0]] = i;
                    continue;
                }

                double d;
                if(double.TryParse(kv[1], out d))
                {
                    data[kv[0]] = d;
                    continue;
                }

                Guid g;
                if(Guid.TryParse(kv[1], out g))
                {
                    data[kv[0]] = g;
                    continue;
                }

                var b = kv[1].ToLower();
                if(b == "true" || b == "yes")
                {
                    data[kv[0]] = true;
                    continue;
                }

                if (b == "false" || b == "no")
                {
                    data[kv[0]] = false;
                    continue;
                }

                data[kv[0]] = kv[1];
            }

            return data;
        }
    }
}
