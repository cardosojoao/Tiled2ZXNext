using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext.Extensions
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
            string result;
            if (value >= 0)
            {
                result = ((int)Math.Round(value, 0)).ToString(format);
            }
            else
            {
                result = ((int)Math.Round(value, 0)).ToString(format).Substring(6, 2);
            }
            return result;
        }

        public static string Int2Hex(this int value, string format = "X2")
        {
            string result;
            if (value >= 0)
            {
                result = value.ToString(format);
            }
            else
            {
                result = value.ToString(format).Substring(6, 2);
            }
            return result;
        }
    }
}
