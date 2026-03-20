using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tiled2dot8.Models
{
    public partial class MetaData
    {
        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Path")]
        public string  Path { get; set; }

        [JsonPropertyName("Modified")]
        public string Modified { get; set; }
        
        [JsonPropertyName("Width")]
        public int Width { get; set; }
        
        [JsonPropertyName("Height")]
        public int Height { get; set; }
        
        [JsonPropertyName("Columns")]
        public int Columns { get; set; }
        
        [JsonPropertyName("Rows")]
        public int Rows { get; set; }

    }
}
