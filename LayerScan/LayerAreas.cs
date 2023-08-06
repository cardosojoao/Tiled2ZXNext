using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext
{
    public class LayerAreas
    {
        public string Name { get; set; }
        public List<Area> Areas { get; set; } = new List<Area>();
        public bool Included(Cell cell)
        {
            if (Areas.Count > 0)
            {
                foreach (Area a in Areas)
                {
                    if (a.Included(cell))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }
    }
}
