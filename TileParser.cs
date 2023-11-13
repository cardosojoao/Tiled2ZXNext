using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;

namespace Tiled2ZXNext
{
    public class TiledParser : TiledRoot
    {
        List<int> spriteSheets;

        public TiledParser()
        {
            spriteSheets = new();
        }


        /// <summary>
        /// get property value as string
        /// </summary>
        /// <param name="properties">collection of properties</param>
        /// <param name="name">property name</param>
        /// <returns>propery value</returns>
        public static string GetProperty(List<Property> properties, string name)
        {
            return properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Value;
        }
        public static string GetProperty(TiledParser tiledData, string name)
        {
            if (tiledData.Properties == null) throw new ArgumentNullException($"tileData.Properties {tiledData.Type}");
            return tiledData.Properties.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        }


        /// <summary>
        /// get property value as int
        /// </summary>
        /// <param name="properties">properties</param>
        /// <param name="name">property name</param>
        /// <returns>value as int in case error 255</returns>
        public static int GetPropertyInt(List<Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException("properties");
            Property? prop  = properties.Find(p => p.Name.Equals( name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? throw new KeyNotFoundException(name) : int.Parse(prop.Value);
        }

        /// <summary>
        /// convert double to hexadecimal value
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>hexadecimal value</returns>
        public static string Double2Hex(double value, string format = "X2")
        {
            string result;
            if (value >= 0)
            {
                result = ((int)Math.Round(value, 0)).ToString(format);
            }
            else
            {
                result = ((int)Math.Round(value, 0)).ToString(format).Substring(6, 2);
            }
            return result;

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
