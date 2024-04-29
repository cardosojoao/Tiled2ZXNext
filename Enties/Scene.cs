using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Entities
{
    public class Scene
    {
        private static Scene _instance;
        public static Scene Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Scene();
                }
                return _instance;
            }
        }
        public string RootFolder { get; set; }
        public string FileName { get; set; }
        public int SpritesPalette { get; set; }
        public int Layer2Palette { get; set; }
        public int TileMapPalette { get; set; }

        public SceneConnector LeftScene { get; set; } = new SceneConnector() { SceneID = 0 };
        public SceneConnector TopScene { get; set; } = new SceneConnector() { SceneID = 0 };
        public SceneConnector RightScene { get; set; } = new SceneConnector() { SceneID = 0 };
        public SceneConnector BottomScene { get; set; } = new SceneConnector() { SceneID = 0 };
        public Dictionary<string, Table> Tables { get; set; } = new();
        public List<Tileset> Tilesets { get; set; } = new List<Tileset>();
        public List<Layer> Layers { get; set; } = new();
        public List<Property> Properties { get; set; } = new();
        public Dictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template> { };

        /// <summary>
        /// resolve GID by set gid to index of specific sprite sheet
        /// </summary>
        /// <param name="gid">Tiled sprite GID</param>
        /// <returns>tupple with gid and sprite sheet converted</returns>
        public (int gid, Tileset tileSheet) GetParsedGid(int gid)
        {
            Tileset tileSheet = null;
            foreach (Tileset tileSet in Tilesets)
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
    }


    public class SceneConnector
    {
        public int SceneID { get; set; }
    }
}
