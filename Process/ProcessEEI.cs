using System;
using System.Text;
using System.Collections.Generic;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Process.EEI;


namespace Tiled2ZXNext
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessEei : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        private readonly List<Entities.Property> _properties;
        public ProcessEei(Layer layer, Scene scene, List<Property> properties)
        {
            _rootLayer = layer;
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Group " + _rootLayer.Name);
            StringBuilder all = new();
            foreach (Layer layer in _rootLayer.Layers)
            {
                if (layer.Visible)
                {
                    Console.WriteLine("Layer " + layer.Name);
                    int type = layer.Properties.GetPropertyInt("Type");
                    switch (type)
                    {
                        case 5:
                            {
                                all.Append(new ProcessPlatform(layer, _scene, _properties).Execute());
                            }
                            break;
                        case 6:
                            {
                                all.Append(new ProcessWatter(layer, _scene, _properties).Execute());
                                break;
                            }
                        case 7:
                            {
                                all.Append(new ProcessScrap(layer, _scene, _properties).Execute());
                                break;
                            }
                    }
                }
            }
            return all;
        }
    }
}