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

        public static List<string> GROUPS = new List<string> { G_LAYER2, G_COLLISION, G_EEI, G_LOCATION, G_LOCATIONS, G_PATH };

        public StringBuilder ProcessScene(Entities.Scene scene)
        {
            StringBuilder layerCode = new(2048);
            string fileName = scene.Properties.GetProperty("FileName");
            layerCode.Append(fileName).AppendLine(":");

            List<IProcess> blocks = CreateProcesses(scene.Layers, scene, null);

            foreach (IProcess process in blocks)
            {
                layerCode.Append(process.Execute());
            }


            // add terminator to scene
            layerCode.Append(fileName);
            layerCode.Append("_eof\r\n");
            layerCode.Append("\t\tdb $");
            layerCode.Append(255.ToString("X2"));
            layerCode.AppendLine("\t\t; end of file");

            return layerCode;
        }

        private List<IProcess> CreateProcesses(List<Entities.Layer> layers, Entities.Scene scene, List<Entities.Property> properties)
        {
            List<IProcess> blocks = new();

            blocks.Add(new ProcessSceneSize(null, scene, properties));


            // get root folders group
            List<Entities.Layer> groups = layers.FindAll(l => l.Type == "group" && l.Visible);

            foreach (Entities.Layer group in groups)
            {
                if (IsGenericGroup(group.Name))
                {
                    blocks.AddRange(CreateProcesses(group.Layers, scene, group.Properties));
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

            return blocks;
        }





        //private StringBuilder ProcessLayers(List<Entities.Layer> layers, Entities.Scene scene, List<Entities.Property> properties)
        //{
        //    List<IProcess> blocks = new();
        //    StringBuilder layerCode = new(2048);

        //    // get root folders group
        //    List<Entities.Layer> groups = layers.FindAll(l => l.Type == "group" && l.Visible);

        //    foreach (Entities.Layer group in groups)
        //    {
        //        if (IsGenericGroup(group.Name))
        //        {
        //            layerCode.Append(ProcessLayers(group.Layers, scene, group.Properties));
        //        }
        //        else if (group.Name.Equals("layer2", System.StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            blocks.Add(new ProcessLayer2(group, scene, properties));
        //            // select Layer 2 group layers
        //        }
        //        else if (group.Name.Equals("collision", System.StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            blocks.Add(new ProcessCollision(group, scene, properties));
        //        }
        //        else if (group.Name.Equals("tilemap", System.StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            blocks.Add(new ProcessTileMap(group, scene, properties));
        //        }
        //        else if (group.Name.Equals("locations", System.StringComparison.InvariantCultureIgnoreCase) || group.Name.Equals("location", System.StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            blocks.Add(new ProcessLocations(group, scene, properties));
        //        }
        //        else if (group.Name.Equals("path", System.StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            blocks.Add(new ProcessPaths(group, scene, properties));
        //        }
        //        else if (group.Name.Equals("EEI", System.StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            blocks.Add(new ProcessEei(group, scene, properties));
        //        }
        //    }

        //    foreach (IProcess process in blocks)
        //    {
        //        layerCode.Append(process.Execute());

        //    }


        //    return layerCode;
        //}


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

    }
}