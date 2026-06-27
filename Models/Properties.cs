using System.Collections.Generic;
using System.Text.Json;
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
        public JsonElement Value { get; set; }

        public object GetValue()
        {
            return Type switch
            {
                "int" => Value.GetInt32(),

                "string" => Value.GetString(),

                "class" => JsonSerializer.Deserialize<Dictionary<string, int>>(Value),
                _ => Value
            };
        }
    }
}

