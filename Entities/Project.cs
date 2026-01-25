using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Tiled2dot8.Entities;
using Tiled2dot8.Models;

namespace Tiled2dot8.Entities
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
