using System;
using System.Text;

namespace sqlQueryWeb.MainLogic
{
    public class MyCrypt
    {
        private const byte CHXOR = (byte) '1';

        public static string Encode(string s, byte byXor = CHXOR)
        {
            var bytes = Encoding.Default.GetBytes(s);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= byXor;
            }
            return Convert.ToBase64String(bytes);
        }

        public static string Decode(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            var bytes = Convert.FromBase64String(s);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= CHXOR;
            }
            return Encoding.Default.GetString(bytes);
        }
    }
}