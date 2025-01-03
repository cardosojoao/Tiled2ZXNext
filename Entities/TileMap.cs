using System.Collections.Generic;

namespace Tiled2ZXNext.Entities
{
    public class TileMap
    {
        public int Type { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public List<Tile> Tiles { get; set; }

        public TileMap(int height, int width)
        {
            Height = height;
            Width = width;
            Tiles = new List<Tile>(height * width);
            for (int i = 0; i < (height * width); i++)
            {
                Tiles.Add(new Tile() { Settings = 0, TileID = 0 });
            }
        }


    }
}
