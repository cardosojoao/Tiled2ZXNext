using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2dot8.Extensions;
using Tiled2dot8.Entities;
using System.IO;
using Model = Tiled2dot8.Models;
using Entity = Tiled2dot8.Entities;
using System.Xml.Serialization;
using Tiled2dot8.Models;
using System.Reflection.Metadata;


namespace Tiled2dot8.Mapper
{
    public class SceneMapper
    {
        public static Entities.Scene Map(Model.Scene sceneRaw, Entity.Scene scene, Options options)
        {

            scene.FileName = sceneRaw.Properties.GetProperty("FileName");
            //scene.LeftScene.SceneID = sceneRaw.Properties.GetPropertyInt("roomleft");
            //scene.RightScene.SceneID = sceneRaw.Properties.GetPropertyInt("roomright");
            //scene.TopScene.SceneID = sceneRaw.Properties.GetPropertyInt("roomtop");
            //scene.BottomScene.SceneID = sceneRaw.Properties.GetPropertyInt("roombottom");
            scene.TileMapPalette = sceneRaw.Properties.GetPropertyInt("PaletteTileMap");
            scene.SpritesPalette = sceneRaw.Properties.GetPropertyInt("PaletteSprite");
            scene.Layer2Palette = sceneRaw.Properties.GetPropertyInt("PaletteLayer2");
            scene.Properties = PropertyMapper.Map(sceneRaw.Properties);


            string inputPath = Path.GetDirectoryName(options.Input);
            scene.RootFolder = inputPath;
            scene.Tilesets = ResolveTileSets(sceneRaw.Tilesets, inputPath);

            //if (sceneRaw.Properties.ExistProperty("Tables"))
            //{
            //    scene.Tables.Append<string, Table>(ResolveTables(sceneRaw.Properties, options.AppRoot));
            //}
            return scene;
        }

        /// <summary>
        /// check if tables are available and load definition
        /// </summary>
        /// <param name="props"></param>
        //private static Dictionary<string, Table> ResolveTables(List<Model.Property> props, string appRoot)
        //{

        //    string[] tablesRaw = props.GetProperty("Tables").Split('\n');           //PropertyExtensions.GetProperty(props, "Tables").Split('\n');
        //    Dictionary<string, Table> tables = new();
        //    foreach (string table in tablesRaw)
        //    {
        //        string[] tableParts = table.Split(':');
        //        Table tableSettings = new Table() { Name = tableParts[0], FilePath = tableParts[1] };
        //        tables.Add(tableSettings.Name, tableSettings);
        //        Console.WriteLine($"Table={tableSettings.Name} Path={tableSettings.FilePath}");
        //        // read the file content    
        //        List<string> tableData = new(File.ReadAllLines(tableSettings.FilePath.Replace("~", appRoot)));
        //        // find table begin
        //        int tableIndex = tableData.FindIndex(r => r.Contains("Table:" + tableSettings.Name, StringComparison.InvariantCultureIgnoreCase));
        //        // if table exists
        //        if (tableIndex > 0)
        //        {   // loop through content until find and empty line
        //            for (int line = tableIndex + 1; line < tableData.Count; line++)
        //            {
        //                string item = tableData[line];
        //                if (item == string.Empty)
        //                {
        //                    break;
        //                }
        //                // get just the name
        //                string[] itemData = item.Split(new char[] { ' ', '\t' });
        //                tableSettings.Items.Add(itemData[0]);
        //            }
        //        }
        //    }
        //    return tables;
        //}

        private static List<Entity.Tileset> ResolveTileSets(List<Model.TileSet> tileSetsRaw, string inputPath)
        {
            Dictionary<string, List<Model.TileSet>> resolved = new();
            Dictionary<string, Model.tileset> tilesSetData = new();

            int order = 0;       // order of load 

            tileSetsRaw.ForEach(t => t.Source = Path.GetFullPath(Path.Combine(inputPath, t.Source)));
            tileSetsRaw = tileSetsRaw.OrderBy(t => t.Source).ToList();

            List < Model.TileSet > tileSets = tileSetsRaw.Where(s=>s.Source.Contains("tilesheets",StringComparison.CurrentCultureIgnoreCase)).ToList();
            List<Model.TileSet> spriteSets = tileSetsRaw.Where(s => s.Source.Contains("sprites", StringComparison.CurrentCultureIgnoreCase)).ToList();

            foreach (Model.TileSet tileSet in tileSets)
            {
                tileSet.Order = order;      // judt to keep in mind the physical order of tilesheet that is align with gid's
                string file = Path.GetFullPath(Path.Combine(inputPath, tileSet.Source));
                Console.Write($"Loading tile sheet {file}.");
                Model.tileset tileSetData = ReadTileSet(file);
                if (tileSetData.properties == null)
                {
                    Console.Error.WriteLine($"Tile sheet {file} has no properties defined, check if was defined at cell level.");
                }
                tileSet.Lastgid = tileSetData.tilecount + tileSet.Firstgid - 1;
                if (!resolved.ContainsKey(tileSetData.image.source))
                {
                    resolved.Add(tileSetData.image.source, new List<Model.TileSet>());
                    tilesSetData.Add(tileSetData.image.source, tileSetData);
                }
                resolved[tileSetData.image.source].Add(tileSet);
                order++;
                Console.WriteLine($"  done.");
            }

            foreach (Model.TileSet tileSet in spriteSets)
            {
                tileSet.Order = order;      // judt to keep in mind the physical order of tilesheet that is align with gid's
                string file = Path.GetFullPath(Path.Combine(inputPath, tileSet.Source));
                Console.Write($"Loading sprite sheet {file}.");
                Model.tileset tileSetData = ReadTileSet(file);
                if (tileSetData.properties == null)
                {
                    Console.Error.WriteLine($"Sprite sheet {file} has no properties defined, check if was defined at cell level.");
                }
                tileSet.Lastgid = tileSetData.tilecount + tileSet.Firstgid - 1;
                if (!resolved.ContainsKey(tileSetData.image.source))
                {
                    resolved.Add(tileSetData.image.source, new List<Model.TileSet>());
                    tilesSetData.Add(tileSetData.image.source, tileSetData);
                }
                resolved[tileSetData.image.source].Add(tileSet);
                order++;
                Console.WriteLine($"  done.");
            }


            List<Entity.Tileset> tilesets = new();


            foreach (KeyValuePair<string, List<Model.TileSet>> sets in resolved)
            {
                Console.Write($"Parse tile/sprite sheet {sets.Key}.");
                foreach (Model.TileSet setRaw in sets.Value)
                {
                    Entity.Tileset tileset = new();
                    tilesets.Add(tileset);
                    tileset.Source = setRaw.Source;
                    tileset.Order = setRaw.Order;
                    tileset.Lastgid = setRaw.Lastgid;
                    tileset.Firstgid = setRaw.Firstgid;
                    tileset.FirstgidMap = setRaw.FirstgidMap;
                    Model.tileset tileSetData = tilesSetData[sets.Key];
                    tileset.Parsedgid = setRaw.Firstgid - 1;
                    //if(tileSetData.properties.ExistProperty("Include") && tileSetData.properties.GetPropertyBool("Include", true))
                    //{
                    //tilesets.Add(tileset);
                    tileset.TileSheetID = tileSetData.properties.GetPropertyInt("TileSheetId");
                    tileset.PaletteIndex = tileSetData.properties.GetPropertyInt("PaletteIndex");
                    //}
                    Console.WriteLine($"  done.");
                }
            }
            return tilesets;
        }


        /// <summary>
        /// load tile set and deserialize into collection of objects
        /// </summary>
        /// <param name="pathFile"></param>
        /// <returns></returns>
        private static Model.tileset ReadTileSet(string pathFile)
        {
            XmlSerializer serializer = new(typeof(Model.tileset));
            Model.tileset tileSet;
            using (Stream reader = new FileStream(pathFile, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                tileSet = (Model.tileset)serializer.Deserialize(reader);
            }
            return tileSet;
        }

        public static Model.XML.Template ReadTemplate(string pathFile)
        {
            XmlSerializer serializer = new(typeof(Model.XML.Template));
            Model.XML.Template template;
            using (Stream reader = new FileStream(pathFile, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                template = (Model.XML.Template)serializer.Deserialize(reader);
            }
            return template;
        }

    }
}
