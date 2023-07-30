using System;
using System.Linq;
using System.Text;

namespace Util
{
    public class Conversion
    {
        public static String HexToString(byte[] from)
        {
            StringBuilder to = new StringBuilder(from.Length * 2);
            foreach (byte b in from)
            {
                to.AppendFormat("{0:x2}", b);
            }
            return to.ToString();
        }

        public static byte[] StringToByteArray(String from) // Hex String to byte array only, use Encoding.ASCII.GetBytes(non Hex string) instead if plain string is needed
        {
            return Enumerable.Range(0, from.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(from.Substring(x, 2), 16))
                     .ToArray();
        }
    }
}