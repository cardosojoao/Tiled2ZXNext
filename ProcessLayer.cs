using System.Collections.Generic;
using System.IO;
using System.Text;
using Tiled2ZXNext.Extensions;
using Model = Tiled2ZXNext.Models;

namespace Tiled2ZXNext
{
    public partial class Controller
    {
        public StringBuilder ProcessLayer(Model.Scene tiledData)
        {
            StringBuilder layerCode = new(2048);
            string fileName = tiledData.Properties.GetProperty( "FileName");

            layerCode.Append(fileName);
            layerCode.Append(":\r\n");

            List<IProcess> blocks = new();
            // get root folders group

            List<Model.Layer> groups = tiledData.Layers.FindAll(l => l.Type == "group" && l.Visible);

            // select Layer 2 group layers
            Model.Layer groupLayer = groups.Find(g => g.Name.Equals("layer2", System.StringComparison.InvariantCultureIgnoreCase));
            if (groupLayer != null)
            {
                blocks.Add(new ProcessLayer2(groupLayer, tiledData));
            }

            // select Collision group layers
            groupLayer = groups.Find(g => g.Name.Equals("collision", System.StringComparison.InvariantCultureIgnoreCase));
            if (groupLayer != null)
            {
                blocks.Add(new ProcessCollision(groupLayer, tiledData));
            }

            // select Tilemap group layers
            groupLayer = groups.Find(g => g.Name.Equals("tilemap", System.StringComparison.InvariantCultureIgnoreCase));
            if (groupLayer != null)
            {
                blocks.Add(new ProcessTileMap(groupLayer, tiledData));
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