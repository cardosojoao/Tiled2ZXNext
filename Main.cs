using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace Tiled2ZXNext
{
    public partial class Controller
    {
        private string inputFile;
        private string fileName;
        private bool compress;
        private string outputFile;
        private string inputPath;

        public string OutputRoomFile { get; set; }
        public string OutputMapFile { get; set; }



        public void Run(Options o)
        {
            inputFile = o.Input;
            compress = o.Compress;

            inputPath = Path.GetDirectoryName(inputFile);
            
            string data = File.ReadAllText(inputFile);


            TiledParser tiledData = JsonSerializer.Deserialize<TiledParser>(data);

            ResolveTileSets(tiledData.Tilesets);

            fileName = TiledParser.GetProperty(tiledData.Properties, "FileName");
            string extension = "asm";
            outputFile = fileName + "." + extension;

            // get map settings
            StringBuilder mapData = ProcessMap(tiledData);
            Console.WriteLine("output map file " + outputFile);
            // write map to final location
            OutputMap(o, mapData);


            BuildList(o.MapPath, "*.asm", o.MapPath + "\\list.txt");

            // get layers data
            StringBuilder layerData = ProcessLayer(tiledData);
            Console.WriteLine("output layer file " + outputFile);
            // write layers to final location
            OutputLayer(o, layerData);

            PostProcessing(o);
        }


        private void ResolveTileSets(List<Tileset> tileSets)
        {
            Dictionary<string, List<Tileset>> resolved = new();
            Dictionary<string, tileset> tilesSetData = new();

            int order= 0;       // order of load 
            foreach (Tileset tileSet in tileSets)
            {
                tileSet.Order = order;      // judt to keep in mind the physical order of tilesheet that is align with gid's
                string file = Path.Combine(inputPath, tileSet.Source);
                tileset tileSetData = ReadTileSet(file);

                tileSet.Lastgid = tileSetData.tilecount + tileSet.Firstgid - 1;
                if (!resolved.ContainsKey(tileSetData.image.source))
                {
                    resolved.Add(tileSetData.image.source, new List<Tileset>());
                    tilesSetData.Add(tileSetData.image.source, tileSetData);
                }
                resolved[tileSetData.image.source].Add(tileSet);
                order++;
            }

            foreach (KeyValuePair<string, List<Tileset>> sets in resolved)
            {
                foreach (Tileset set in sets.Value)
                {
                    tileset tileSetData = tilesSetData[sets.Key];
                    set.Parsedgid = set.Firstgid - 1;


                    set.TileSheetID = int.Parse(GetTileSetProperty(tileSetData.properties, "TileSheetId"));
                    set.PaletteIndex = int.Parse(GetTileSetProperty(tileSetData.properties, "PaletteIndex"));

                    // if tile properties are setup, get the palette index and TileSheetID (checking if they are defined)
                    if (tileSetData.tile.properties != null)
                    {
                        if (ExistProperty(tileSetData.tile.properties, "TileSheetId"))
                        {
                            set.TileSheetID = int.Parse(GetTileSetProperty(tileSetData.tile.properties, "TileSheetId"));
                        }
                        if(ExistProperty(tileSetData.tile.properties, "PaletteIndex"))
                        {
                            set.PaletteIndex = int.Parse(GetTileSetProperty(tileSetData.tile.properties, "PaletteIndex"));
                        }
                        
                    }
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
                if (prop.name.ToLower() == name.ToLower())
                {
                    value = prop.value;
                    break;
                }
            }
            return value;
        }

        /// <summary>
        /// check if property exists
        /// </summary>
        /// <param name="properties">list of properties</param>
        /// <param name="name">property name</param>
        /// <returns></returns>
        private static bool ExistProperty(TilesetTileProperty[] properties, string name)
        {
            bool found = false;
            foreach (TilesetTileProperty prop in properties)
            {
                if (prop.name.ToLower() == name.ToLower())
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
}
