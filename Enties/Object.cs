using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tiled2ZXNext.Entities
{
    public class Object
    {
        public double Height { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public double Width { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

}
