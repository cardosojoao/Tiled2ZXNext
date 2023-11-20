using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using Tiled2ZXNext.Extensions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Tiled2ZXNext.Mappers;
using Tiled2ZXNext.Enties;
using Tiled2ZXNext.Models;

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

        //public static readonly Dictionary<string, Table> Tables = new();

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

            //ResolveTileSets(tiledData.Tilesets);
            //ResolveTables(tiledData.Properties);

            fileName = tiledData.Properties.GetProperty( "FileName");
            string extension = "asm";
            outputFile = fileName + "." + extension;

            Enties.Scene scene = SceneMapper.Map(tiledData, _options);      // migrated

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
        /// get the sprite sheet id from tileset
        /// </summary>
        /// <param name="properties">collection of properties</param>
        /// <param name="name">property name</param>
        /// <returns>property value or empty if not found</returns>
        //private static string GetTileSetProperty(tileset tileSet, string name)
        //{
        //    if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
        //    TilesetTileProperty prop = tileSet.properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        //    return prop?.value ?? "";
        //}


        //private static string GetMapProperty(List<Property> properties, string name)
        //{
        //    if (properties == null) throw new ArgumentNullException($"missing [properties]");
        //    Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        //    return prop?.Value ?? "";
        //}

        /// <summary>
        /// check if property exists
        /// </summary>
        /// <param name="properties">list of properties</param>
        /// <param name="name">property name</param>
        /// <returns></returns>
        //private static bool ExistProperty(tileset tileSet, string name)
        //{
        //    if (tileSet.properties == null) throw new ArgumentNullException($"tileSet: missing [properties] of {tileSet.name}");
        //    TilesetTileProperty prop = tileSet.properties.FirstOrDefault(p => p.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        //    return prop == null ? false : true;
        //}

        //private static bool ExistProperty(List<Property> properties, string name)
        //{
        //    if (properties == null) throw new ArgumentNullException($"missing [properties]");
        //    Property prop = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        //    return prop != null;
        //}

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


}
