using System;
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
    }
}