using System.Collections.Generic;
using Tiled2dot8.Entities;

namespace Tiled2dot8.Entities
{
    public class Template
    {
        public int Gid { get; set; }
        public string Type { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Property> Properties { get; set; } = new List<Property>();
    }
}
