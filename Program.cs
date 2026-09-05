using CommandLine;
using System;
using System.Reflection;
using Tiled2dot8.Metadata;

namespace Tiled2dot8
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {

                string version = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

                Console.WriteLine($"Tiled 2 Dot8 {version}");

                Parser.Default.ParseArguments<SceneOptions, PatternsOptions, WorldOptions, TileSheetOptions,SpritetOptions>(args)
                    .WithParsed<SceneOptions>(options =>
                    {
                        if (options.Verbose)
                        {
                            Console.WriteLine($"Verbose output enabled. Current Arguments: -v {options.Verbose}");
                            Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                        }
                        else
                        {
                            Console.WriteLine($"Verbose output enabled. Current Arguments: -v {options.Verbose}");
                            Controller main = new();
                            main.Run(options);
                        }
                    })
                    .WithParsed<PatternsOptions>(PatternsOptions =>
                    {
                        Console.WriteLine($"Verbose output enabled. Current Arguments: -v {PatternsOptions.Verbose}");
                        MetadataUpdate main = new();
                        main.Run(PatternsOptions);
                    })
                    .WithParsed<WorldOptions>(WorldOptions =>
                    {
                        Console.WriteLine($"Verbose output enabled. Current Arguments: -v {WorldOptions.Verbose}");
                        WorldUpdate main = new();
                        main.Run(WorldOptions);
                    })
                    .WithParsed<TileSheetOptions>(TileSetOptions =>
                    {
                        Console.WriteLine($"Verbose output enabled. Current Arguments: -v {TileSetOptions.Verbose}");
                        TileSheetUpdate main = new();
                        main.Run(TileSetOptions);
                    })
                    .WithParsed<SpritetOptions>(SpriteOptions =>
                    {
                        Console.WriteLine($"Verbose output enabled. Current Arguments: -v {SpriteOptions.Verbose}");
                        SpriteUpdate main = new();
                        main.Run(SpriteOptions);
                    })
                    .WithNotParsed(errors =>
                    {
                        Environment.Exit(1);
                    });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
