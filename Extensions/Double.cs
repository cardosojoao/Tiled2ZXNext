using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext.Extensions
{
    public static  class Double
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

    }
}
