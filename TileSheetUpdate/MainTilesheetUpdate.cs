using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entities = TiledIO.Entities;
using TiledIO.Mapper;
using TiledIO.Models;
using Model = TiledIO.Models;

namespace Tiled2dot8
{
    public class TileSheetUpdate
    {

        public static int GetTileSheetProperty(tileset tileSheet, string propertyName = "TileSheetId")
        {
            if (tileSheet.properties == null) return int.MaxValue;
            var p = tileSheet.properties.FirstOrDefault(x => string.Equals(x.name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (p != null && int.TryParse(p.value, out var v))
                return v;
            return int.MaxValue;
        }
        public static void SetTileSheetProperty(tileset tileSheet,string propertyName, string type, string value, string propertyType = "")
        {
            if (tileSheet.properties == null)
            {
                tileSheet.properties = new Model.TilesetTileProperty[1] { new Model.TilesetTileProperty
                    {
                        name = propertyName,
                        type = type,
                        value = value

                    }
                };
                if (propertyType != "")
                {
                    tileSheet.properties[0].propertytype = propertyType;
                }
            }
            else
            {
                var prop = tileSheet.properties.FirstOrDefault(x => string.Equals(x.name, propertyName, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                {
                    var props = tileSheet.properties;
                    Array.Resize(ref props, tileSheet.properties.Length + 1);
                    prop = new Model.TilesetTileProperty
                    {
                        name = propertyName,
                        type = type,
                        value = value
                    };
                    if (propertyType != "")
                    {
                        prop.propertytype = propertyType;
                    }

                    props[^1] = prop;
                    tileSheet.properties = props;
                }
                else
                {
                    prop.type = type;
                    prop.value = value;
                }
            }
        }


        public void Run(TileSheetOptions args)
        {
            string[] tileSheetsPath = Directory.GetFiles(Path.GetFullPath(args.Input), "*.tsx", SearchOption.AllDirectories);
            List<(string path, Model.tileset tileSheet)> tileSheets = new List<(string path, Model.tileset tileSheet)>(tileSheetsPath.Length);
            foreach (string tileSheetPath in tileSheetsPath)
            {
                Model.tileset tileSetData = SceneMapper.ReadTileSet(tileSheetPath);
                if (tileSetData.properties == null)
                {
                    tileSetData.properties = new Model.TilesetTileProperty[0];
                }

                if (GetTileSheetProperty(tileSetData, "TileSheetId") == int.MaxValue)
                {
                    SetTileSheetProperty(tileSetData, "TileSheetId", "int", "1024");
                }
                if (GetTileSheetProperty(tileSetData, "Type") == int.MaxValue)
                {
                    SetTileSheetProperty(tileSetData, "Type", "int", "0", "TileSetType");
                }
                if (GetTileSheetProperty(tileSetData, "Type") == 0)
                {
                    tileSheets.Add((path: tileSheetPath, tileSheet: tileSetData));
                }
            }

            tileSheets.Sort((a, b) => GetTileSheetProperty(a.tileSheet, "TileSheetId").CompareTo(GetTileSheetProperty(b.tileSheet, "TileSheetId")));

            int firstId = 0;
            for (int index = 0; index < tileSheets.Count; index++)
            {
                tileset tileSheet = tileSheets[index].tileSheet;
                if (GetTileSheetProperty(tileSheet) != firstId)
                {
                    Console.WriteLine($"Updating {tileSheets[index].path} TileSheetId from {GetTileSheetProperty(tileSheet)} to {firstId}");
                    SetTileSheetProperty(tileSheet, "TileSheetId", "int", firstId.ToString());
                    SceneMapper.WriteTileSet(tileSheet, tileSheets[index].path);
                }
                else
                {
                    Console.WriteLine($"TileSheetId for {tileSheets[index].path} is already {firstId}");
                }
                firstId++;
            }

        }
    }
}