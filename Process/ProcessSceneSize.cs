using System;
using System.Collections.Generic;
using System.Text;
using Tiled2dot8.Entities;

namespace Tiled2dot8
{
    public class ProcessSceneSize : IProcess
    {
        private readonly Scene _scene;
        private readonly List<Property> _properties;
        public ProcessSceneSize(Layer layer, Scene scene, List<Property> properties)
        {
            _scene = scene;
            _properties = properties;
        }

        public StringBuilder Execute()
        {
            Console.WriteLine("Scene size");
            int blockType = 13;
            StringBuilder sceneSize = new();
            Entities.Layer layer = _scene.Layers.Find(l => l.Name.Equals("layer2", StringComparison.InvariantCultureIgnoreCase));
            int width = layer.Layers[0].Width;
            int height = layer.Layers[0].Height;
            sceneSize.Append("\t\tdb $").Append(blockType.ToString("X2")).Append("\t\t; data block type Scene Size\r\n");
            sceneSize.Append("\t\tdw $").Append(2.ToString("X4")).Append("\t\t; Block size\r\n");
            sceneSize.Append("\t\tdb $").Append(width.ToString("X2")).Append(", $").Append(height.ToString("X2")).AppendLine("\t\t; scene size width, height");
            return sceneSize;
        }


    }
}
