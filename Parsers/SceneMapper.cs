using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiled2ZXNext.Extensions;
using Tiled2ZXNext.Enties;
using System.IO;
using Model = Tiled2ZXNext.Models;
using System.Xml.Serialization;
using Tiled2ZXNext.Models;

namespace Tiled2ZXNext.Mappers
{
    public class SceneMapper
    {
        public static Enties.Scene Map(Model.Scene tiledData, Options options)
        {
            Enties.Scene scene = new()
            {
                FileName = tiledData.Properties.GetProperty("FileName")
            };
            scene.LeftScene.SceneID = tiledData.Properties.GetPropertyInt("roomleft");
            scene.RightScene.SceneID = tiledData.Properties.GetPropertyInt("roomright");
            scene.TopScene.SceneID = tiledData.Properties.GetPropertyInt("roomtop");
            scene.BottomScene.SceneID = tiledData.Properties.GetPropertyInt("roombottom");
            scene.TileMapPalette = tiledData.Properties.GetPropertyInt("PaletteTileMap");
            scene.SpritesPalette = tiledData.Properties.GetPropertyInt("PaletteSprite");
            scene.Layer2Palette = tiledData.Properties.GetPropertyInt("PaletteLayer2");

            ResolveTileSets(tiledData.Tilesets, options.Input);

            if (tiledData.Properties.ExistProperty("Tables"))
            {
                scene.Tables.Append<string, Table>(ResolveTables(tiledData.Properties, options.AppRoot));
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

        private static void ResolveTileSets(List<Tileset> tileSets, string inputPath)
        {
            Dictionary<string, List<Tileset>> resolved = new();
            Dictionary<string, TileSetXMl> tilesSetData = new();

            int order = 0;       // order of load 
            foreach (Tileset tileSet in tileSets)
            {
                tileSet.Order = order;      // judt to keep in mind the physical order of tilesheet that is align with gid's
                string file = Path.Combine(inputPath, tileSet.Source);
                TileSetXMl tileSetData = ReadTileSet(file);

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
                    TileSetXMl tileSetData = tilesSetData[sets.Key];
                    set.Parsedgid = set.Firstgid - 1;
                    set.TileSheetID = tileSetData.GetPropertyInt("TileSheetId");        //  unique id of tilesheet
                    set.PaletteIndex = tileSetData.GetPropertyInt("PaletteIndex");      //  palette id or -1 if no palette 
                }
            }
        }

        /// <summary>
        /// load tile set and deserialize into collection of objects
        /// </summary>
        /// <param name="pathFile"></param>
        /// <returns></returns>
        private static TileSetXMl ReadTileSet(string pathFile)
        {
            XmlSerializer serializer = new(typeof(TileSetXMl));
            TileSetXMl tileSet;
            using (Stream reader = new FileStream(pathFile, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                tileSet = (TileSetXMl)serializer.Deserialize(reader);
            }
            return tileSet;
        }

    }
}
