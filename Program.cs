using CommandLine;
using System;
using System.Reflection;

namespace Tiled2ZXNext
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                Console.WriteLine("Tiled 2 ZX Next converter " + Assembly.GetEntryAssembly().GetName().Version);

                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        if (o.Verbose)
                        {
                            Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                            Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                        }
                        else
                        {
                            Controller main = new();
                            main.Run(o);
                        }
                    });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}
