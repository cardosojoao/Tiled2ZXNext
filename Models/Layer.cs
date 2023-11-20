using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tiled2ZXNext.Models
{
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
}
