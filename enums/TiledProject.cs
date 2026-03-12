using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2dot8.enums
{
    public class TiledProject
    {
        public List<TiledEnum> PropertyTypes { get; set; }
    }

    public class TiledEnum
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }   // "enum"
        public List<string> Values { get; set; }
        public bool ValuesAsFlags { get; set; }
    }
}
