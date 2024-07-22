using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using Tiled2ZXNext.Extensions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Models;
using Tiled2ZXNext.Mapper;

namespace Tiled2ZXNext
{
    public partial class Controller
    {
        private string inputFile;
        private string fileName;
        private string outputFile;

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



            if (o.World.Length > 0)
            {
                if (File.Exists(o.World))
                {
                    string worldraw = File.ReadAllText(o.World);
                    Models.World worldData = JsonSerializer.Deserialize<Models.World>(worldraw);
                    Entities.World world = WorldMapper.Map(worldData, o);

                    world.GetMatrix();

                    ProcessWorld proc =  new ProcessWorld(world);

                    var worldProc =  proc.Execute();

                    File.WriteAllText(Path.Combine(o.MapPath, "worldMap.asm"), worldProc.ToString());

                    
                }
            }

            //inputPath = Path.GetDirectoryName(inputFile);
            string data = File.ReadAllText(inputFile);

            Models.Scene sceneRaw = JsonSerializer.Deserialize<Models.Scene>(data);


            fileName = sceneRaw.Properties.GetProperty("FileName");
            string extension = "asm";
            outputFile = fileName + "." + extension;

            Entities.Scene scene = SceneMapper.Map(sceneRaw, Entities.Scene.Instance, _options);      // migrated
            scene.Layers = LayerMapper.Map(sceneRaw.Layers);

            // Check templates



            // get map settings
            //StringBuilder mapData = ProcessMap(scene);
            //Console.WriteLine("output map file " + outputFile);
            // write map to final location
            //OutputMap(o, mapData);


            //BuildList(o.MapPath, "*.asm", o.MapPath + "\\list.txt");

            // get layers data
            StringBuilder layerData = ProcessScene(scene);
            Console.WriteLine("output layer file " + outputFile);
            // write layers to final location
            OutputLayer(o, layerData);

            PostProcessing(o);
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


}
