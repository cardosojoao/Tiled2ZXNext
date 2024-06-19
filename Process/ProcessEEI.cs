using System;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;


namespace Tiled2ZXNext
{
    /// <summary>
    /// Process Environment Element Interaction
    /// </summary>
    public class ProcessEEI : IProcess
    {
        private readonly Layer _rootLayer;
        private readonly Scene _scene;
        public ProcessEEI(Layer layer, Scene scene)
        {
            _rootLayer = layer;
            _scene = scene;
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
                                all.Append(new ProcessPlatform(layer, _scene).Execute());
                            }
                            break;
                        case 6:
                            {
                                all.Append(new ProcessWatter(layer, _scene).Execute());
                                break;
                            }
                    }
                }
            }
            return all;
        }
    }
}