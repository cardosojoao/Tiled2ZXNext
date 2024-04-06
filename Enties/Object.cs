using System;
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
        public string Type { get; set; }
        public Polygon Polygon { get; set; }
    }


    public class Polygon : List<PolygonPoint>
    {
        public List<Property> Properties { get; set; }
    }

    public class PolygonPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

}
