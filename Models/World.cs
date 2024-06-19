using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tiled2ZXNext.Models
{

    public class Map
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class World
    {
        [JsonPropertyName("maps")]
        public List<Map> Maps { get; set; }

        [JsonPropertyName("onlyShowAdjacentMaps")]
        public bool OnlyShowAdjacentMaps { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }



}
