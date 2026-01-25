using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tiled2dot8.Entities;

namespace Tiled2dot8.Models
{

    public class Project
    {
        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; }
        [JsonPropertyName("propertyTypes")]
        public List<PropertyType> PropertyTypes { get; set; }
    }
}
