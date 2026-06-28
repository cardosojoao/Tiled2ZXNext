using System;
using System.Collections.Generic;
using TiledIO.Entities;

namespace Tiled2dot8
{
    public class Utils
    {
        public static Layer Find(List<Layer> layers, string name)
        {
            Layer found = layers.Find(l => l.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (found == null)
            {
                foreach(Layer  layer in layers)
                {
                    if (layer.Layers == null || layer.Layers.Count == 0)
                        continue;
                        
                    found = Find(layer.Layers, name);
                    if (found != null)
                    {
                        break;
                    }
                }
            }
            return found;
        }
    }
}
