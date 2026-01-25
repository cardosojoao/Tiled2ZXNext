using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2dot8.Extensions
{
    public static  class Hexa
    {

        /// <summary>
        /// convert double to hexadecimal value
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>hexadecimal value</returns>
        public static string Double2Hex(this double value, string format = "X2")
        {
            short shortNumber = (short)value;
            string result = shortNumber.ToString(format);
            return result;
        }

        public static string Int2Hex(this Int32 value, string format = "X2")
        {
            // Ensure the number fits within 2 bytes (16 bits)
            short shortNumber = (short)value;
            string result =  shortNumber.ToString(format);
            return result;
        }


        public static string Byte2Hex(this Int32 value, string format = "X2")
        {
            short shortNumber = (short)value;
            string result = shortNumber.ToString(format)[^2..];
            return result;
        }
    }
}
