using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tiled2ZXNext
{
    public partial class Controller
    {
        public StringBuilder ProcessLayer(TiledParser tiledData)
        {
            StringBuilder full = new(2048);
            string fileName = TiledParser.GetProperty(tiledData.Properties, "FileName");

            full.Append(fileName);
            full.Append(":\r\n");

            // we going to process the layers by type
            List<Layer> tileLayers = tiledData.Layers.FindAll(l => l.Type == "tilelayer");
            if (tileLayers.Count > 0)
            {
                TileMap tilemap = new(tileLayers[0].Height, tileLayers[0].Width);
                foreach (Layer layer in tileLayers)
                {
                    if (layer.Visible)
                    {
                        tiledData.ParseLayer(layer, tilemap);
                    }
                }
                StringBuilder tile = tiledData.WriteTiledLayer(tilemap);
                full.Append(tile);
            }

            List<Layer> objectGroup = tiledData.Layers.FindAll(l => l.Type == "objectgroup");
            foreach (Layer layer in objectGroup)
            {
                if (layer.Visible)
                {
                    full.Append(fileName);
                    full.Append('_');
                    full.Append(layer.Name);
                    full.Append('_');
                    full.Append(layer.Id);
                    full.Append(":\r\n");

                    full.Append(tiledData.WriteObjectsLayer(layer));
                }
            }
            // add terminator to scene
            full.Append(fileName);
            full.Append("_eof\r\n");
            full.Append("\t\tdb $");
            full.Append(255.ToString("X2"));
            full.Append("\t\t; end of file\r\n");

            return full;
        }


        public void OutputLayer(Options o, StringBuilder mapData)
        {
            string pathOutput = Path.Combine(o.RoomPath, outputFile);
            File.WriteAllText(pathOutput, mapData.ToString());
        }
    }
}