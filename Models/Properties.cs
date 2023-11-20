using System.Text.Json.Serialization;

namespace Tiled2ZXNext.Models
{
    public class Property
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
