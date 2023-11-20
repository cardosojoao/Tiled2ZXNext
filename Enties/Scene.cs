using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2ZXNext.Enties
{
    public class Scene
    {
        public string FileName {  get; set; }
        public int  SpritesPalette {  get; set; }   
        public int Layer2Palette {  get; set; }
        public int TileMapPalette { get; set; }
        
        public SceneConnector LeftScene {  get; set; } = new SceneConnector() { SceneID = 0};
        public SceneConnector TopScene { get; set; } = new SceneConnector() { SceneID = 0 };
        public SceneConnector RightScene { get; set; } = new SceneConnector() { SceneID = 0 };
        public SceneConnector BottomScene { get;set; } = new SceneConnector() { SceneID = 0 };
        public Dictionary<string,Table> Tables { get; set; } = new();
    }


    public class SceneConnector
    {
        public int SceneID { get; set; }
    }
}
