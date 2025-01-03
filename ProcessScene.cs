using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Tiled2ZXNext.Entities;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Models;

namespace Tiled2ZXNext
{
    public partial class Controller
    {
        public static string G_LAYER2 = "layer2";
        public static string G_COLLISION = "collision";
        public static string G_LOCATION = "location";
        public static string G_LOCATIONS = "locations";
        public static string G_PATH = "path";
        public static string G_EEI = "eei";
        public static string G_BACKGROUND = "background";

        public static List<string> GROUPS = new List<string> { G_LAYER2, G_COLLISION, G_EEI, G_LOCATION, G_LOCATIONS, G_PATH , G_BACKGROUND};

        public StringBuilder ProcessScene(Entities.Scene scene)
        {
            StringBuilder processesCode = new(4096);
            StringBuilder sceneCode = new(2048);

            string fileName = scene.Properties.GetProperty("FileName");
            sceneCode.Append(fileName).AppendLine(":");

            List<IProcess> blocks = CreateProcesses(scene.Layers, scene, null);

            foreach (IProcess process in blocks)
            {
                processesCode.Append(process.Execute());
            }

            sceneCode.Append(processesCode);

            // add terminator to scene
            sceneCode.Append(fileName);
            sceneCode.Append("_eof\r\n");
            sceneCode.Append("\t\tdb $");
            sceneCode.Append(255.ToString("X2"));
            sceneCode.AppendLine("\t\t; end of file");

            return sceneCode;
        }

        private List<IProcess> CreateProcesses(List<Entities.Layer> layers, Entities.Scene scene, List<Entities.Property> properties)
        {
            List<IProcess> blocks = new();
            blocks.Add(new ProcessBackgroundColour(null, scene, properties));
            blocks.Add(new ProcessSceneSize(null, scene, properties));




            // get root folders group
            List<Entities.Layer> groups = layers.FindAll(l => l.Type == "group" && l.Visible);
            foreach (Entities.Layer group in groups)
            {
                if (group.Layers.Count > 0)
                {
                    if (IsGenericGroup(group.Name))     // used for recursive call
                    {
                        blocks.AddRange(CreateProcesses(group.Layers, scene, group.Properties));
                    }
                    else if (group.Name.Equals("background", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        blocks.Add(new ProcessFillArea(group, scene, properties));
                    }
                    else if (group.Name.Equals("layer2", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        blocks.Add(new ProcessLayer2(group, scene, properties));
                        // select Layer 2 group layers
                    }
                    else if (group.Name.Equals("collision", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        blocks.Add(new ProcessCollision(group, scene, properties));
                    }
                    else if (group.Name.Equals("tilemap", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        blocks.Add(new ProcessTileMap(group, scene, properties));
                    }
                    else if (group.Name.Equals("locations", System.StringComparison.InvariantCultureIgnoreCase) || group.Name.Equals("location", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        blocks.Add(new ProcessLocations(group, scene, properties));
                    }
                    else if (group.Name.Equals("path", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        blocks.Add(new ProcessPaths(group, scene, properties));
                    }
                    else if (group.Name.Equals("EEI", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        blocks.Add(new ProcessEei(group, scene, properties));
                    }
                }
            }
            return blocks;
        }

        public void OutputLayer(Options o, string worldName, StringBuilder mapData)
        {
            string FolderOutput = Path.Combine(o.RoomPath, worldName);
            if(!Directory.Exists(FolderOutput))
            {
                Directory.CreateDirectory(FolderOutput);
            }
            string pathOutput = Path.Combine(FolderOutput, outputFile);
            File.WriteAllText(pathOutput, mapData.ToString());
        }

        private List<Entities.Layer> GetGenericGroups(List<Entities.Layer> layers)
        {
            List<Entities.Layer> groupsGeneric = layers.FindAll(l => GROUPS.FindIndex(g => g == l.Name) > -1);
            return groupsGeneric;
        }

        private static bool IsGenericGroup(string name)
        {
            return GROUPS.FindIndex(g => g.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)) == -1;
        }

        public static void AddSceneHeaderCode(StringBuilder code, int bytes)
        {

        }
    }
}