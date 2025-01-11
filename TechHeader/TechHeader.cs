using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext
{
    internal static class TechHeader
    {
        private static Dictionary<string, int> _objectsCount = new();
        public static void Reset()
        {
            _objectsCount.Clear();

        }


        public static StringBuilder Get()
        {
            StringBuilder _sb = new();

            int maxkey = 0;
            foreach(string key in  _objectsCount.Keys)
            {
                maxkey = int.Max(key.Length, maxkey);
            }
            _sb.AppendLine(";\tObjects allocation:");
            foreach (KeyValuePair<string, int> kvp in _objectsCount)
            {
                _sb.Append(";\t\t").Append(kvp.Key).Append(new string(' ', maxkey-kvp.Key.Length)).Append("\t").AppendFormat( "{0,2}", kvp.Value).AppendLine();
            }
            return _sb;
        }

        public static void Add(string  key, int counter)
        {
            if (_objectsCount.ContainsKey(key))
            {
                _objectsCount[key] += counter;
            }
            else
            {
                _objectsCount.Add(key, counter);
            }
        }
    }
}
