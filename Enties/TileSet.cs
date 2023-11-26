namespace Tiled2ZXNext.Entities
{

    public class Tileset
    {
        public int Firstgid { get; set; }

        /// <summary>
        /// first GID to be reported to parser
        /// </summary>
        public int FirstgidMap { get; set; }

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