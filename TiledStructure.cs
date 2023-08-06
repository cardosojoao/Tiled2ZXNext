using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tiled2ZXNext
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Property
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class Object
    {
        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; }

        [JsonPropertyName("rotation")]
        public int Rotation { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("gid")]
        public int? Gid { get; set; }
    }

    public partial class Layer
    {
        [JsonPropertyName("layers")]
        public List<Layer> Layers { get; set; }

        [JsonPropertyName("data")]
        public List<uint> Data { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("draworder")]
        public string Draworder { get; set; }

        [JsonPropertyName("objects")]
        public List<Object> Objects { get; set; }

        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; }
    }

    public partial class Tileset
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
        public int Parsedgid { get; set;  }
        /// <summary>
        /// Sprite sheet id ( each sprite sheet is 8K and can have 32 sprites of 16x16 or 128 of 8x8)
        /// </summary>
        public int TileSheetID { get; set; }
        public int PaletteIndex { get; set; }

        public int Order { get; set; }

    }

    public class TiledRoot
    {
        [JsonPropertyName("compressionlevel")]
        public int Compressionlevel { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("infinite")]
        public bool Infinite { get; set; }

        [JsonPropertyName("layers")]
        public List<Layer> Layers { get; set; }

        [JsonPropertyName("nextlayerid")]
        public int Nextlayerid { get; set; }

        [JsonPropertyName("nextobjectid")]
        public int Nextobjectid { get; set; }

        [JsonPropertyName("orientation")]
        public string Orientation { get; set; }

        [JsonPropertyName("renderorder")]
        public string Renderorder { get; set; }

        [JsonPropertyName("tiledversion")]
        public string Tiledversion { get; set; }

        [JsonPropertyName("tileheight")]
        public int Tileheight { get; set; }

        [JsonPropertyName("tilesets")]
        public List<Tileset> Tilesets { get; set; }

        [JsonPropertyName("tilewidth")]
        public int Tilewidth { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; }
    }


}
