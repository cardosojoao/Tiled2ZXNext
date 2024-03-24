using System.Collections.Generic;
using System.IO;
using System.Text;
using Tiled2ZXNext.Extensions;

namespace Tiled2ZXNext
{
    public partial class Controller
    {
        public StringBuilder ProcessLayer(Entities.Scene scene)
        {
            StringBuilder layerCode = new(2048);
            string fileName = scene.Properties.GetProperty( "FileName");

            layerCode.Append(fileName);
            layerCode.Append(":\r\n");

            List<IProcess> blocks = new();
            // get root folders group
            List<Entities.Layer> groups = scene.Layers.FindAll(l => l.Type == "group" && l.Visible);

            // select Layer 2 group layers
            Entities.Layer groupLayer = groups.Find(g => g.Name.Equals("layer2", System.StringComparison.InvariantCultureIgnoreCase));
            if (groupLayer != null)
            {
                blocks.Add(new ProcessLayer2(groupLayer, scene));
            }

            // select Collision group layers
            groupLayer = groups.Find(g => g.Name.Equals("collision", System.StringComparison.InvariantCultureIgnoreCase));
            if (groupLayer != null)
            {
                blocks.Add(new ProcessCollision(groupLayer, scene));
            }

            // select Tilemap group layers
            groupLayer = groups.Find(g => g.Name.Equals("tilemap", System.StringComparison.InvariantCultureIgnoreCase));
            if (groupLayer != null)
            {
                blocks.Add(new ProcessTileMap(groupLayer, scene));
            }

            // select Tilemap group layers
            groupLayer = groups.Find(g => g.Name.Equals("locations", System.StringComparison.InvariantCultureIgnoreCase));
            if (groupLayer != null)
            {
                blocks.Add(new ProcessLocations(groupLayer, scene));
            }


            foreach (IProcess process in blocks)
            {
                layerCode.Append(process.Execute());
            }
            // add terminator to scene
            layerCode.Append(fileName);
            layerCode.Append("_eof\r\n");
            layerCode.Append("\t\tdb $");
            layerCode.Append(255.ToString("X2"));
            layerCode.Append("\t\t; end of file\r\n");

            return layerCode;
        }


        public void OutputLayer(Options o, StringBuilder mapData)
        {
            string pathOutput = Path.Combine(o.RoomPath, outputFile);
            File.WriteAllText(pathOutput, mapData.ToString());
        }
    }
}