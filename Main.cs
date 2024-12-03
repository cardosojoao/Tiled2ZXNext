using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
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

            string[] worlds = Directory.GetFiles(Path.GetDirectoryName(o.World), "*.world");
            foreach (string worldFile in worlds)
            {
                string worldRaw = File.ReadAllText(worldFile);
                Models.World worldData = JsonSerializer.Deserialize<Models.World>(worldRaw);
                Entities.World world = WorldMapper.Map(worldData, o);
                world.Name = Path.GetFileNameWithoutExtension(worldFile);

                world.GetMatrix();

                // world.GetMatrix();
                ProcessWorld proc = new ProcessWorld(world);
                var worldProc = proc.Execute();
                string worldName = Path.GetFileName(worldFile);
                File.WriteAllText(Path.Combine(o.MapPath, Path.GetFileNameWithoutExtension(worldName)+"_worldMap.asm"), worldProc.ToString());
            }



            if (o.Project.Length > 0)
            {
                if (File.Exists(o.Project))
                {
                    string projectRaw = File.ReadAllText(o.Project);
                    Models.Project projectData = JsonSerializer.Deserialize<Models.Project>(projectRaw);
                    ProjectMapper.Map(projectData, Entities.Project.Instance, o);
                }
            }
            string data = File.ReadAllText(inputFile);
            Models.Scene sceneRaw = JsonSerializer.Deserialize<Models.Scene>(data);
            fileName = sceneRaw.Properties.GetProperty("FileName");
            string extension = "asm";
            outputFile = fileName + "." + extension;

            Entities.Scene scene = SceneMapper.Map(sceneRaw, Entities.Scene.Instance, _options);      // migrated
            scene.Layers = LayerMapper.Map(sceneRaw.Layers);
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
            }

            layerData.Insert(0, header);
            string sceneWorldName = scene.Properties.GetProperty("WorldName");
            
            Console.WriteLine("output layer file " + outputFile);
            // write layers to final location
            OutputLayer(o, sceneWorldName, layerData);
            PostProcessing(o, sceneWorldName);
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
