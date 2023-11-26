using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext.Entities
{
    public class Table
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<string> Items { get; set; } = new();
    }
}
