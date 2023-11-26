using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tiled2ZXNext.Models
{
    public partial class TileSet
    {
        [JsonPropertyName("firstgid")]
        public int Firstgid { get; set; }

        /// <summary>
        /// first GID to be reported to parser
        /// </summary>
        public int FirstgidMap { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        public int Lastgid { get; set; }
        /// <summary>
        /// result of resolving the tileset names, different tileset could have the same image, just different tile size
        /// what should be the value substracted to the gid value
        /// </summary>
        public int Parsedgid { get; set; }
        /// <summary>
        /// Sprite sheet id ( each sprite sheet is 8K and can have 32 sprites of 16x16 or 128 of 8x8)
        /// </summary>
        public int TileSheetID { get; set; }
        public int PaletteIndex { get; set; }

        public int Order { get; set; }

    }
}
