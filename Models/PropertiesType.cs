using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tiled2ZXNext.Models
{
    public class PropertyType
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("storageType")]
        public string StorageType { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("values")]
        public List<string> Values { get; set; }

        [JsonPropertyName("valuesAsFlags")]
        public bool ValuesAsFlags { get; set; }
    }
}
