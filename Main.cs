using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Tiled2dot8.Palette;
using TiledIO.Entities;
using TiledIO.Extensions;
using TiledIO.Mapper;
using Entities = TiledIO.Entities;
using Models = TiledIO.Models;

namespace Tiled2dot8
{
    public partial class Controller
    {
        private string inputFile;
        private string fileName;
        private string outputFile;

        public string OutputRoomFile { get; set; }
        public string OutputMapFile { get; set; }
        private SceneOptions _options;
        public static Config Config { get; set; } = new();
        public static GPL Palette { get; set; }
        public void Run(SceneOptions o)
        {

            //DebugTest();



            if (o.Input != null)
            {
                _options = o;
                inputFile = o.Input;
                IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appconfig.json", optional: true, reloadOnChange: false)
                    .Build();
                Config.Offset = config.GetRequiredSection("offset").Get<Offset>();
                Config.Zip = config.GetRequiredSection("zip").Get<Command>();
                Config.Assembler = config.GetRequiredSection("assembler").Get<Command>();
                Config.SpriteSoftwareSuffix = config.GetValue<string>("SpriteSoftwareSuffix");

                if (o.Project.Length > 0)
                {
                    if (File.Exists(o.Project))
                    {
                        string projectRaw = File.ReadAllText(o.Project);
                        Models.Project projectData = JsonSerializer.Deserialize<Models.Project>(projectRaw);
                        ProjectMapper.Map(projectData, Entities.Project.Instance, o.Input, o.AppRoot);
                    }
                }

                Scene scene  = TiledIO.Tiled.LoadScene(inputFile);

                fileName = scene.Properties.GetProperty("SceneName");
                string extension = "asm";
                outputFile = fileName + "." + extension;
                // palette require for background color and fill area
                Palette = LoadGplPalette.Load(o.PalettePath);

                // get layers data
                StringBuilder layerData = ProcessScene(scene);
                StringBuilder header = new();

                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName().Name;
                var assemblyVersion = assembly.GetName().Version;

                header.AppendLine(";");
                header.Append(";\t").AppendLine($"Assembly Name: {assemblyName}");
                header.Append(";\t").AppendLine($"Assembly Version: {assemblyVersion}");
                header.AppendLine(";");

                foreach (Table table in Entities.Project.Instance.Includes.Values)
                {
                    string tablePath = table.FilePath.Replace("~", o.AppRoot);
                    string path = Path.GetRelativePath(o.RoomPath, tablePath);
                    header.Append('\t').Append("include\t\"").Append(tablePath.Replace('\\', '/')).AppendLine("\"");
                    Console.WriteLine("include " + path);
                }

                layerData.Insert(0, header);
               string sceneWorldName = scene.Properties.GetProperty("WorldName");

                Console.WriteLine("output layer file " + outputFile);
                // write layers to final location
                OutputLayer(o, sceneWorldName, layerData);
                PostProcessing(o, sceneWorldName);
            }
        }


        private void DebugTest()
        {
            Layer layer = new();
            layer.Width = 10;
            layer.Height = 10;
            layer.Data = new List<uint>(100){ 
                0,0,0,0,0,0,0,0,0,0,
                0,1,1,1,0,0,0,0,0,0,
                0,1,1,1,0,0,0,0,0,0,
                0,1,1,1,0,0,0,0,0,0,
                0,1,1,0,0,0,0,0,0,0,
                0,1,1,0,0,0,0,0,0,0,
                0,1,1,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,
                0,1,1,1,0,0,0,0,0,0,
                0,1,1,1,0,0,0,0,0,0
            };

            LayerScanFill scan = new(layer);
            scan.Scan();
             


        }
    }


    public class Config
    {
        public string? SpriteSoftwareSuffix { get; set; }
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
