using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tiled2ZXNext.Entities;


namespace Tiled2ZXNext
{
    public partial class Controller
    {
        public void PostProcessing(Options o)
        {
            int execResult = -1;
            string outputfileAssembler = Path.Combine(o.RoomPath, fileName + ".bin");
            string inputFile = Path.Combine(o.RoomPath, outputFile);

            if (Controller.Config.Assembler != null)
            {
                string pathExe = Controller.Config.Assembler.App; // assemblerConfig.GetSection("app").Value;
                string param = Controller.Config.Assembler.Args; // assemblerConfig.GetSection("args").Value;

                param = param.Replace("%1", inputFile).Replace("%2", outputfileAssembler);
                string tables = "";
                if ("" == string.Empty)
                {

                }

                param.Replace("%3", tables);
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

            BuildList(o.RoomPath, "*.zx0", o.RoomPath + "\\list.txt");

        }

        static int ExecuteCommand(string exec, string parameters)
        {
            Console.WriteLine("execute  {0} {1}", exec, parameters);
            ProcessStartInfo startinfo = new ProcessStartInfo(exec, parameters);
            startinfo.ErrorDialog = true;
            startinfo.CreateNoWindow = true;
            startinfo.UseShellExecute = true;
            System.Diagnostics.Process myProcess = System.Diagnostics.Process.Start(startinfo);
            myProcess.Start();
            myProcess.WaitForExit(5000);
            int result = myProcess.ExitCode;

            myProcess.Close();
            myProcess.Dispose();

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
