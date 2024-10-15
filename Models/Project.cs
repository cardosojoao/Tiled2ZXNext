using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tiled2ZXNext.Entities;

namespace Tiled2ZXNext.Models
{

    public class Project
    {
        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; }
        [JsonPropertyName("propertyTypes")]
        public List<PropertyType> PropertyTypes { get; set; }
    }
}
