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
                Console.WriteLine("Tiled 2 Dot8 " + Assembly.GetEntryAssembly().GetName().Version);

                Parser.Default.ParseArguments<SceneOptions, PatternsOptions, WorldOptions>(args)
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

                    .WithNotParsed(errors => { });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}
