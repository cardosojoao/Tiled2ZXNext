using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Entities
{
    public class Project
    {
        private static Project _instance;
        public static Project Instance
        {
            get
            {
                _instance ??= new Project();
                return _instance;
            }
        }
        public string RootFolder { get; set; }
        public List<Property> Properties { get; set; } = new();
        public Dictionary<string, Table> Tables { get; set; } = new();
        public Dictionary<string, Table> Includes { get; set; } = new();
        public List<PropertyType> PropertyTypes { get; set; } = new();
    }
}
