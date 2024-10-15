using System.Collections.Generic;

namespace Tiled2ZXNext.Entities
{
    /// <summary>
    /// project global properties, currently only allow enums
    /// </summary>
    public class PropertyType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string StorageType { get; set; }

        public string Type { get; set; }

        public List<string> Values { get; set; }

        public bool ValuesAsFlags { get; set; }


    }
}

