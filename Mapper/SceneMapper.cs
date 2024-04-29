using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Entities;
using System.IO;
using Model = Tiled2ZXNext.Models;
using Entity = Tiled2ZXNext.Entities;
using System.Xml.Serialization;
using Tiled2ZXNext.Models;
using System.Reflection.Metadata;


namespace Tiled2ZXNext.Mapper
{
    public class SceneMapper
    {
        public static Entities.Scene Map(Model.Scene sceneRaw, Entity.Scene scene, Options options)
        {

            scene.FileName = sceneRaw.Properties.GetProperty("FileName");
            scene.LeftScene.SceneID = sceneRaw.Properties.GetPropertyInt("roomleft");
            scene.RightScene.SceneID = sceneRaw.Properties.GetPropertyInt("roomright");
            scene.TopScene.SceneID = sceneRaw.Properties.GetPropertyInt("roomtop");
            scene.BottomScene.SceneID = sceneRaw.Properties.GetPropertyInt("roombottom");
            scene.TileMapPalette = sceneRaw.Properties.GetPropertyInt("PaletteTileMap");
            scene.SpritesPalette = sceneRaw.Properties.GetPropertyInt("PaletteSprite");
            scene.Layer2Palette = sceneRaw.Properties.GetPropertyInt("PaletteLayer2");
            scene.Properties = PropertyMapper.Map(sceneRaw.Properties);


            string inputPath = Path.GetDirectoryName(options.Input);
            scene.RootFolder = inputPath;
            scene.Tilesets = ResolveTileSets(sceneRaw.Tilesets, inputPath);

            if (sceneRaw.Properties.ExistProperty("Tables"))
            {
                scene.Tables.Append<string, Table>(ResolveTables(sceneRaw.Properties, options.AppRoot));
            }
            return scene;
        }

        /// <summary>
        /// check if tables are available and load definition
        /// </summary>
        /// <param name="props"></param>
        private static Dictionary<string, Table> ResolveTables(List<Model.Property> props, string appRoot)
        {

            string[] tablesRaw = props.GetProperty("Tables").Split('\n');           //PropertyExtensions.GetProperty(props, "Tables").Split('\n');
            Dictionary<string, Table> tables = new();
            foreach (string table in tablesRaw)
            {
                string[] tableParts = table.Split(':');
                Table tableSettings = new Table() { Name = tableParts[0], FilePath = tableParts[1] };
                tables.Add(tableSettings.Name, tableSettings);
                Console.WriteLine($"Table={tableSettings.Name} Path={tableSettings.FilePath}");
                // read the file content    
                List<string> tableData = new(File.ReadAllLines(tableSettings.FilePath.Replace("~", appRoot)));
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
            return tables;
        }

        //private static void ResolveTileSetsold(List<Model.Tileset> tileSetsRaw, string inputPath)
        //{
        //    //Dictionary<string, List<Tileset>> resolved = new();

        //    Dictionary<string, TileSetXMl> tilesSetData = new();

        //    int order = 0;       // order of load 
        //    foreach (Model.Tileset tileSetRaw in tileSetsRaw)
        //    {
        //        Entity.Tileset tileset = new();
        //        tileset.Order = order;                  // judt to keep in mind the physical order of tilesheet that is align with gid's
        //        //tileSetRaw.Order = order;      
        //        string file = Path.Combine(inputPath, tileSetRaw.Source);
        //        TileSetXMl tileSetData = ReadTileSet(file);
        //        tileset.Lastgid = tileSetData.tilecount + tileSetRaw.Firstgid - 1;
        //        //tileSetRaw.Lastgid = tileSetData.tilecount + tileSetRaw.Firstgid - 1;
        //        if (!Entity.Scene.Instance.Tilesets.ContainsKey(tileSetData.image.source))
        //        {
        //            Entity.Scene.Instance.Tilesets.Add(tileSetData.image.source, new List<Entity.Tileset>());
        //            tilesSetData.Add(tileSetData.image.source, tileSetData);
        //        }
        //        Entity.Scene.Instance.Tilesets[tileSetData.image.source].Add(tileset);
        //        order++;
        //    }

        //    foreach (KeyValuePair<string, List<Entity.Tileset>> sets in Entity.Scene.Instance.Tilesets)
        //    {
        //        foreach (Entity.Tileset set in sets.Value)
        //        {
        //            TileSetXMl tileSetData = tilesSetData[sets.Key];
        //            set.Parsedgid = set.Firstgid - 1;
        //            set.TileSheetID = tileSetData.GetPropertyInt("TileSheetId");        //  unique id of tilesheet
        //            set.PaletteIndex = tileSetData.GetPropertyInt("PaletteIndex");      //  palette id or -1 if no palette 
        //        }
        //    }
        //}



        private static List<Entity.Tileset> ResolveTileSets(List<Model.TileSet> tileSetsRaw, string inputPath)
        {
            Dictionary<string, List<Model.TileSet>> resolved = new();
            Dictionary<string, Model.tileset> tilesSetData = new();

            int order = 0;       // order of load 
            foreach (Model.TileSet tileSet in tileSetsRaw)
            {
                tileSet.Order = order;      // judt to keep in mind the physical order of tilesheet that is align with gid's
                string file = Path.Combine(inputPath, tileSet.Source);
                Model.tileset tileSetData = ReadTileSet(file);

                tileSet.Lastgid = tileSetData.tilecount + tileSet.Firstgid - 1;
                if (!resolved.ContainsKey(tileSetData.image.source))
                {
                    resolved.Add(tileSetData.image.source, new List<Model.TileSet>());
                    tilesSetData.Add(tileSetData.image.source, tileSetData);
                }
                resolved[tileSetData.image.source].Add(tileSet);
                order++;
            }
            List<Entity.Tileset> tilesets = new();


            foreach (KeyValuePair<string, List<Model.TileSet>> sets in resolved)
            {
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
                    tileset.TileSheetID = tileSetData.properties.GetPropertyInt("TileSheetId");
                    tileset.PaletteIndex = tileSetData.properties.GetPropertyInt("PaletteIndex");
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
