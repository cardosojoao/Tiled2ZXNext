using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tiled2dot8.Entities;


namespace Tiled2dot8
{
    public partial class Controller
    {
        public void PostProcessing(Options o, string worldName)
        {
            int execResult = -1;
            string outputPath = Path.Combine(o.RoomPath, worldName);
            string outputfileAssembler = Path.Combine(outputPath, fileName + ".bin");
            string inputFile = Path.Combine(outputPath, outputFile);

            if (Controller.Config.Assembler != null)
            {
                string pathExe = Controller.Config.Assembler.App; // assemblerConfig.GetSection("app").Value;
                string param = Controller.Config.Assembler.Args; // assemblerConfig.GetSection("args").Value;

                param = param.Replace("%1", inputFile).Replace("%2", outputfileAssembler);
                execResult = ExecuteCommand(pathExe, param);
                Console.WriteLine($"Result={execResult}");
            }

            if (Controller.Config.Zip != null && execResult == 0)
            {
                string pathExe = Controller.Config.Zip.App;     // zipConfig.GetSection("app").Value;
                string param = Controller.Config.Zip.Args;      // zipConfig.GetSection("args").Value;

                string outputfileCompress = Path.Combine(Path.GetDirectoryName(outputfileAssembler), Path.GetFileNameWithoutExtension(outputfileAssembler) + ".zx0");

                param = param.Replace("%1", outputfileAssembler).Replace("%2", outputfileCompress);

                execResult = ExecuteCommand(pathExe, param);
                Console.WriteLine($"Result={execResult}");
            }
            else
            {
                Console.WriteLine("ERROR: Skip compress step");
            }

            BuildList(outputPath, "*.zx0", outputPath + "\\list.txt");

        }

        static int ExecuteCommand(string exec, string parameters)
        {
            Console.WriteLine("execute  {0} {1}", exec, parameters);
            ProcessStartInfo startinfo = new ProcessStartInfo(exec, parameters);
            startinfo.ErrorDialog = true;
            startinfo.CreateNoWindow = true;
            startinfo.UseShellExecute = false;
            startinfo.RedirectStandardOutput = true;
            startinfo.RedirectStandardError = true;




            System.Diagnostics.Process myProcess = System.Diagnostics.Process.Start(startinfo);

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            // Event handlers for async output/error reading
            myProcess.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    output.AppendLine( args.Data);
            };

            myProcess.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    error.AppendLine( args.Data);
            };



            myProcess.Start();

            myProcess.BeginOutputReadLine();
            myProcess.BeginErrorReadLine();


            myProcess.WaitForExit(5000);
            int result = myProcess.ExitCode;

            myProcess.Close();
            myProcess.Dispose();

            Console.WriteLine(output.ToString());
            Console.WriteLine(error.ToString());

            return result;
        }


        private void BuildList(string path, string filter, string outputFile)
        {
            var listData = Directory.EnumerateFiles(path, filter);

            StringBuilder listOut = new(1000);
            List<string> listSort = new(255);

            foreach (string file in listData)
            {
                listSort.Add(Path.GetFileName(file));
            }

            listSort.Sort();

            foreach (string file in listSort)
            {
                listOut.Append(file);
                listOut.Append("\r\n");
            }
            Console.WriteLine("output file " + outputFile);
            File.WriteAllText(outputFile, listOut.ToString());
        }
    }
}
