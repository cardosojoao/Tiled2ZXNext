using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tiled2dot8.Entities;
using Tiled2dot8.Mapper;
using Tiled2dot8.Models;

namespace Tiled2dot8.Metadata
{
    public class MetadataUpdate
    {

        public void Run(PatternsOptions args)
        {
            Console.WriteLine($"Metadata update.");

            string[] paths = args.Input.Split(',');

            foreach(string  path in paths)
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine($"Folder {path}");
                    IEnumerable<string> files = Directory.EnumerateFiles(path, "*.tsx", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        Console.WriteLine($"File {file}");
                        Update(file);
                    }
                }
                else
                {
                    Console.WriteLine($"Path {path} does not exist.");
                }
            };
        }

        public void Update(List<Tileset> tileSets)
        {
            foreach (Tileset tileSet in tileSets)
            {
                Update(tileSet.Source);
            }
        }



        public void Update(string filePathTileSet)
        {
            Entities.Metadata metaData = null;
            tileset tileSet = SceneMapper.ReadTileSet(filePathTileSet);
            string pathData = Path.Combine(Path.GetDirectoryName(filePathTileSet), tileSet.image.source);
            string PathMetadata = GetMetadataFilePath(pathData);
            if (!File.Exists(PathMetadata))
            {
                metaData = new();
                DateTime modified = File.GetLastWriteTime(pathData);
                metaData.Enabled = true;
                metaData.Modified = modified.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
                metaData.Path = pathData;
            }
            else
            {
                string metadataRaw = File.ReadAllText(PathMetadata);
                Models.MetaData MetaDataData = JsonSerializer.Deserialize<Models.MetaData>(metadataRaw);
                metaData = MetadataMapper.Map(MetaDataData);
            }
            metaData.Enabled = true;
            metaData.Width = tileSet.tilewidth;
            metaData.Height = tileSet.tileheight;
            metaData.Columns = tileSet.columns;
            metaData.Rows = tileSet.tilecount / tileSet.columns;
            metaData.Path = pathData;
            string metadataJson = JsonSerializer.Serialize(metaData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PathMetadata, metadataJson);
        }


        public static string GetMetadataFilePath(string inputPath)
        {
            string directory = Path.GetDirectoryName(inputPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputPath);
            return Path.Combine(directory, fileNameWithoutExtension + ".metadata");
        }
    }
}