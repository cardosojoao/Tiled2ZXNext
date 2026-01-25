using System.Text.Json.Serialization;

namespace Tiled2dot8.Models
{
    public class Property
    {
        
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("propertytype")]
        public string Propertytype { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
}
