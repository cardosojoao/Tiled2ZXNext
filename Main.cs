using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace Tiled2ZXNext
{
    public partial class Controller
    {
        private string inputFile;
        private string fileName;
        private string outputFile;
        private string inputPath;

        public string OutputRoomFile { get; set; }
        public string OutputMapFile { get; set; }

        public static readonly Dictionary<string, Table> Tables = new();

        private Options _options;
        public static Config Config { get; set; } = new();


        public void Run(Options o)
        {
            _options = o;
            inputFile = o.Input;



            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json", optional: true, reloadOnChange: false)
                .Build();
            Config.Offset = config.GetRequiredSection("offset").Get<Offset>();
            Config.Zip = config.GetRequiredSection("zip").Get<Command>();
            Config.Assembler = config.GetRequiredSection("assembler").Get<Command>();

            inputPath = Path.GetDirectoryName(inputFile);
            string data = File.ReadAllText(inputFile);

            TiledParser tiledData = JsonSerializer.Deserialize<TiledParser>(data);

            ResolveTileSets(tiledData.Tilesets);
            ResolveTables(tiledData.Properties);

            fileName = TiledParser.GetProperty(tiledData, "FileName");
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

        /// <summary>
        /// check if tables are available and load definition
        /// </summary>
        /// <param name="props"></param>
        private void ResolveTables(List<Property> props)
        {
            if (ExistMapProperty(props, "Tables"))
            {
                string[] tables = GetMapProperty(props, "Tables").Split('\n');
                foreach (string table in tables)
                {
                    string[] tableParts = table.Split(':');
                    Table tableSettings = new Table() { Name = tableParts[0], FilePath = tableParts[1] };
                    Tables.Add(tableSettings.Name, tableSettings);
                    Console.WriteLine($"Table={tableSettings.Name} Path={tableSettings.FilePath}");
                    // read the file content    
                    List<string> tableData = new(File.ReadAllLines(tableSettings.FilePath.Replace("~", _options.AppRoot)));
                    // find table begin
                    int tableIndex = tableData.FindIndex(r => r.Contains("Table:" + tableSettings.Name, StringComparison.InvariantCultureIgnoreCase));
                    // if table exists
                    if (tableIndex > 0)
                    {   // loop through content until find and empty line
                        for (int line = tableIndex + 1; line < tableData.Count; line++)
                        {
                            string item = tableData[line];
                            if (item == string.Empty)
                            {
                                break;
                            }
                            // get just the name
                            string[] itemData = item.Split(new char[] { ' ', '\t' });
                            tableSettings.Items.Add(itemData[0]);
                        }
                    }
                }
            }
        }


        private void ResolveTileSets(List<Tileset> tileSets)
        {
            Dictionary<string, List<Tileset>> resolved = new();
            Dictionary<string, tileset> tilesSetData = new();

            int order = 0;       // order of load 
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


                    set.TileSheetID = int.Parse(GetTileSetProperty(tileSetData, "TileSheetId"));        //  unique id of tilesheet
                    set.PaletteIndex = int.Parse(GetTileSetProperty(tileSetData, "PaletteIndex"));      //  palette id or -1 if no palette 

                    // if tile properties are setup, get the palette index and TileSheetID (checking if they are defined)
                    //if (tileSetData.tile.properties != null)
                    //{
                    //    if (ExistProperty(tileSetData.tile.properties, "TileSheetId"))
                    //    {
                    //        set.TileSheetID = int.Parse(GetTileSetProperty(tileSetData.tile.properties, "TileSheetId"));
                    //    }
                    //    if (ExistProperty(tileSetData.tile.properties, "PaletteIndex"))
                    //    {
                    //        set.PaletteIndex = int.Parse(GetTileSetProperty(tileSetData.tile.properties, "PaletteIndex"));
                    //    }

                    //}
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
        private static string GetTileSetProperty(tileset tileSet, string name)
        {
            if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
            TilesetTileProperty prop = tileSet.properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.value ?? "";

            //foreach (TilesetTileProperty prop in properties)
            //{
            //    if (prop.name.ToLower() == name.ToLower())
            //    {
            //        value = prop.value;
            //        break;
            //    }
            //}
            //return value;
        }


        private static string GetMapProperty(List<Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException($"missing [properties]");
            Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop?.Value ?? "";
            //string value = string.Empty;
            //foreach (Property prop in properties)
            //{
            //    if (prop.Name.ToLower() == name.ToLower())
            //    {
            //        value = prop.Value;
            //        break;
            //    }
            //}
            //return value;
        }

        /// <summary>
        /// check if property exists
        /// </summary>
        /// <param name="properties">list of properties</param>
        /// <param name="name">property name</param>
        /// <returns></returns>
        private static bool ExistProperty(tileset tileSet, string name)
        {
            if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
            TilesetTileProperty prop = tileSet.properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? false : true;

            //bool found = false;
            //foreach (TilesetTileProperty prop in properties)
            //{
            //    if (prop.name.ToLower() == name.ToLower())
            //    {
            //        found = true;
            //        break;
            //    }
            //}
            //return found;
        }

        private static bool ExistMapProperty(List<Property> properties, string name)
        {
            if (properties == null) throw new ArgumentNullException($"missing [properties]");
            Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return prop == null ? false : true;

            //bool found = false;
            //foreach (Property prop in properties)
            //{
            //    if (prop.Name.ToLower() == name.ToLower())
            //    {
            //        found = true;
            //        break;
            //    }
            //}
            //return found;
        }

    }


    public class Config
    {
        public Offset Offset { get; set; }
        public Command? Zip { get; set; }
        public Command? Assembler { get; set; }
    }


    public class Command
    {
        public string? App { get; set; }
        public string? Args { get; set; }
    }
    public class Offset
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class Table
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<string> Items { get; set; } = new();
    }
}
