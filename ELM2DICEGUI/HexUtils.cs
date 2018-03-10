using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELM2DICEGUI
{
    class HexUtils
    {
        public static byte[] hexStringToByteArray(string hexstring)
        {
            byte[] retval = new byte[(hexstring.Length + 2) / 3];
            int idx = 0;
            foreach (string hexpair in hexstring.Split(' '))
            {
                retval[idx] = Byte.Parse(hexpair, NumberStyles.HexNumber);
                idx++;
            }

            return retval;
        }

        public static string byteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
