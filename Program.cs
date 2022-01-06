﻿using System;

using System.Reflection;

namespace Tiled2ZXNext
{
    class Program
    {
        static void Main(string[] args)
        {

            ImageHelper.t(@"D:\devl\zx\assets\tilemap8x8_8.png", @"D:\devl\zx\assets\tilemap8x8_8t.png");


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

                    Controller main = new (inputFile, compress);
                    main.Run();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }
        }
    }
}
