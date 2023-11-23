using System;
using System.Collections.Generic;

namespace Tiled2ZXNext.Entities
{
    public partial class Layer
    {
        public List<Layer> Layers { get; set; }

        public List<uint> Data { get; set; }

        public int Height { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool Visible { get; set; }

        public int Width { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public string Draworder { get; set; }

        public List<Object> Objects { get; set; }

        public List<Property> Properties { get; set; }
    }
}
