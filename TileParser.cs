using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using Tiled2ZXNext.Models;

namespace Tiled2ZXNext
{
    public class TiledParser : Scene
    {
        List<int> spriteSheets;

        public TiledParser()
        {
            spriteSheets = new();
        }






        

        /// <summary>
        /// resolve GID by set gid to index of specific sprite sheet
        /// </summary>
        /// <param name="gid">Tiled sprite GID</param>
        /// <returns>tupple with gid and sprite sheet converted</returns>
        public  (int gid, Tileset tileSheet) GetParsedGid(int gid)
        {
            Tileset tileSheet = null;
            foreach (Tileset tileSet in this.Tilesets)
            {
                if (gid >= tileSet.Firstgid && gid <= tileSet.Lastgid)
                {
                    // we are going to load the two tilesheets in memory sequential the number can be sequential //  gid -= tileSet.Parsedgid;
                    tileSheet = tileSet;
                    break;
                }
            }
            return (gid, tileSheet);
        }

        /// <summary>
        /// keep track of all different Sprite sheets where used in the current block
        /// </summary>
        /// <param name="spriteSheets">collection of used sprite sheets</param>
        /// <param name="spriteSheetID">current sprite sheet ID</param>
        private static void SpriteSheetsBlock(List<int> spriteSheets, int spriteSheetID)
        {
            if (!spriteSheets.Exists(i => i == spriteSheetID))
            {
                spriteSheets.Add(spriteSheetID);
            }
        }
    }


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
    public class Tile
    {
        public uint Settings;
        public uint TileID;
    }
}
