using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext.Entities
{
    /// <summary>
    /// single tile
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// properties like rotate, vertical mirror and horizontal mirror
        /// </summary>
        public uint Settings {  get; set; }
        /// <summary>
        /// tile id mapped to TileSheet
        /// </summary>
        public uint TileID {  get; set; }
    }
}
