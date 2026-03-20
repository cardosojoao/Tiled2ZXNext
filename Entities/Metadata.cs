using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2dot8.Entities
{
    public class Metadata
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Modified { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }

    }
}
