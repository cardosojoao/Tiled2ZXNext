using System.Collections.Generic;

namespace Tiled2ZXNext.Entities
{
    /// <summary>
    /// Layer of areas, instead of a matrix of cells, stores a collections of areas that contains cells
    /// the objective is to be able to store the same amount of data using less data
    /// </summary>
    public class LayerAreas
    {
        /// <summary>
        /// Name of Layer
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Collections of areas
        /// </summary>
        public List<Area> Areas { get; set; } = new List<Area>();

        public List<Tileset> TileSet { get; set; }


    }
}
