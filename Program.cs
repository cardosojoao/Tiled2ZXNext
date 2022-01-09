using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace Tiled2ZXNext
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Tiled 2 ZX Next converter " + Assembly.GetEntryAssembly().GetName().Version);


            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json", optional: true, reloadOnChange: false)
                .Build();

            var zipConfig = config.GetSection("zip");
            var assemblerConfig = config.GetSection("assembler");


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
                        if (param == "-c")
                        {
                            compress = true;
                        }
                    }

                    Console.WriteLine("Input file " + args[0]);
                    Console.WriteLine("Processing ");

                    String inputFile = args[0];

                    Controller main = new(inputFile, compress);
                    main.Run();

                    int execResult = -1;
                    string outputfileAssembler = Path.Combine( Path.GetDirectoryName(main.OutputFile),  Path.GetFileNameWithoutExtension(main.OutputFile) + ".bin");
                    if (assemblerConfig != null)
                    {
                        string pathExe = assemblerConfig.GetSection("app").Value;
                        string param = assemblerConfig.GetSection("args").Value;

                        param =  param.Replace("%1", main.OutputFile).Replace("%2", outputfileAssembler);

                        execResult = ExecuteCommand(pathExe , param);
                    }

                    if(zipConfig != null && execResult == 0)
                    {
                        string pathExe = zipConfig.GetSection("app").Value;
                        string param = zipConfig.GetSection("args").Value;
                        
                        string outputfileCompress = Path.Combine(Path.GetDirectoryName(outputfileAssembler), Path.GetFileNameWithoutExtension(outputfileAssembler) + ".zx0");

                        param = param.Replace("%1", outputfileAssembler).Replace("%2", outputfileCompress);

                        execResult = ExecuteCommand(pathExe, param);
                    }

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }
        }



        static int ExecuteCommand(string exec, string parameters)
        {
            ProcessStartInfo startinfo = new ProcessStartInfo(exec, parameters);
            startinfo.ErrorDialog = true;
            startinfo.CreateNoWindow = true;
            startinfo.UseShellExecute = true;
            Process myProcess = Process.Start(startinfo);
            myProcess.Start();
            myProcess.WaitForExit(5000);
            int result = myProcess.ExitCode;

            myProcess.Close();
            myProcess.Dispose();

            return result;
        }
    }
}
