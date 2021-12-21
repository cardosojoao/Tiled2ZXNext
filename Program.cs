using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Reflection;

namespace Tiled2ZXNext
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Tiled 2 ZX Next converter " + Assembly.GetEntryAssembly().GetName().Version);
            if (args.Length == 0)
            {
                Console.WriteLine("No input file.");
            }
            else
            {
                try
                {
                    bool compress = false;
                    if (args.Length > 1)
                    {
                        string param = args[1].ToLower();
                        if(param == "-c")
                        {
                            compress = true;
                        }
                    }
                    
                    Console.WriteLine("Input file " + args[0]);
                    Console.WriteLine("Processing ");

                    String inputFile = args[0];
                    string outputPath = Path.GetDirectoryName(inputFile);
                    string data = File.ReadAllText(inputFile);

                    TiledParser tiledData = JsonSerializer.Deserialize<TiledParser>(data);

                    StringBuilder full = new StringBuilder(2048);
                    string fileName = TiledParser.GetProperty(tiledData.Properties, "FileName");
                    string extension = "asm";

                    
                    full.Append(fileName);
                    full.Append(":\r\n");

                    foreach (Layer layer in tiledData.Layers)
                    {
                        if (layer.Visible)
                        {
                            full.Append(fileName);
                            full.Append('_');
                            full.Append(layer.Name);
                            full.Append(":\r\n");

                            full.Append(tiledData.WriteLayer(layer, compress));
                        }
                    }

                    fileName = Path.ChangeExtension(fileName, extension);
                    string fullPath = Path.Combine(outputPath, fileName);
                    Console.WriteLine("output file " + fullPath);
                    File.WriteAllText(fullPath, full.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
