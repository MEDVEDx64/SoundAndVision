using System;

namespace Massacre.Snv.Core.Network.Utils
{
    public static class Base64
    {
        public static string Encode(string s)
        {
            return Convert.ToBase64String(PacketBase.Encoding.GetBytes(s)).Replace('=', '$');
        }

        public static string Decode(string s)
        {
            return PacketBase.Encoding.GetString(Convert.FromBase64String(s.Replace('$', '=')));
        }
    }
}
