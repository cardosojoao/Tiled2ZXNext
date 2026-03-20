using System;
using System.Text;
using System.Collections.Generic;
using Tiled2dot8.Entities;
using Tiled2dot8.Extensions;
using Tiled2dot8.Process.EEI;

namespace Tiled2dot8
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
                        case 17:
                            {
                                all.Append(new ProcessObjectComponents(layer, _scene, _properties).Execute());
                                break;
                            }
                    }
                }
            }
            return all;
        }
    }
}