using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace Tiled2ZXNext
{
    public class Controller
    {
        private readonly string inputFile;
        private readonly bool compress;
        private readonly string outputPath;

        public Controller(string inputFile, bool compress)
        {
            this.inputFile = inputFile;
            this.compress = compress;
            outputPath = Path.GetDirectoryName(inputFile);
        }

        public void Run()
        {
            //string outputPath = Path.GetDirectoryName(inputFile);
            string data = File.ReadAllText(inputFile);

            TiledParser tiledData = JsonSerializer.Deserialize<TiledParser>(data);

            ResolveTileSets(tiledData.Tilesets);

            StringBuilder full = new(2048);
            string fileName = TiledParser.GetProperty(tiledData.Properties, "FileName");
            string extension = "asm";


            full.Append(fileName);
            full.Append(":\r\n");

            foreach (Layer layer in tiledData.Layers)
            {
                if (layer.Visible)
                {
                    full.Append(fileName);
                    full.Append('_');
                    full.Append(layer.Name);
                    full.Append(":\r\n");

                    full.Append(tiledData.WriteLayer(layer, compress));
                }
            }
            full.Append(fileName);
            full.Append("_eof\r\n");
            full.Append("\t\tdb $");
            full.Append(255.ToString("X2"));
            full.Append("\t\t; end of file\r\n");


            fileName = Path.ChangeExtension(fileName, extension);
            string fullPath = Path.Combine(outputPath, fileName);
            Console.WriteLine("output file " + fullPath);
            File.WriteAllText(fullPath, full.ToString());
        }


        private void ResolveTileSets(List<Tileset> tileSets)
        {
            Dictionary<string, List<Tileset>> resolved = new();
            Dictionary<string, tileset> tilesSetData = new();

            foreach (Tileset tileSet in tileSets)
            {
                string file = Path.Combine(outputPath, tileSet.Source);
                tileset tileSetData = ReadTileSet(file);

                tileSet.Lastgid = tileSetData.tilecount + tileSet.Firstgid - 1;
                if (!resolved.ContainsKey(tileSetData.image.source))
                {
                    resolved.Add(tileSetData.image.source, new List<Tileset>());
                    tilesSetData.Add(tileSetData.image.source, tileSetData);
                }
                resolved[tileSetData.image.source].Add(tileSet);
            }

            foreach (KeyValuePair<string, List<Tileset>> sets in resolved)
            {
                foreach (Tileset set in sets.Value)
                {
                    tileset tileSetData = tilesSetData[sets.Key];
                    set.Parsedgid = set.Firstgid - 1;
                    // set sprite sheet id
                    set.SpriteSheetID = int.Parse( GetTileSetProperty(tileSetData.properties, "SpriteSheetID"));
                }
            }
        }

        /// <summary>
        /// load tile set and deserialize into collection of objects
        /// </summary>
        /// <param name="pathFile"></param>
        /// <returns></returns>
        private static tileset ReadTileSet(string pathFile)
        {
            XmlSerializer serializer = new(typeof(tileset));
            tileset tileSet;
            using (Stream reader = new FileStream(pathFile, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                tileSet = (tileset)serializer.Deserialize(reader);
            }
            return tileSet;
        }

        /// <summary>
        /// get the sprite sheet id from tileset
        /// </summary>
        /// <param name="properties">collection of properties</param>
        /// <param name="name">property name</param>
        /// <returns>property value or empty if not found</returns>
        private static string GetTileSetProperty(TilesetTileProperty[] properties, string name)
        {
            string value = string.Empty;
            foreach (TilesetTileProperty prop in properties)
            {
                if(prop.name.ToLower() == name.ToLower())
                {
                    value = prop.value;
                    break;
                }
            }
            return value;
        }
    }
}
