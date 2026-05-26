using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2dot8.Entities
{
    /// <summary>
    /// single tile
    /// </summary>
    public class TileSetTile
    {
        /// <summary>
        /// properties l
        /// </summary>
        public List<Property> Properties { get; set; }
        /// <summary>
        /// tile id mapped to TileSheet
        /// </summary>
        public uint ID {  get; set; }
    }
}
