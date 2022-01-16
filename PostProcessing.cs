using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tiled2ZXNext
{
    public partial class Controller
    {

        public void PostProcessing(Options o)
        {
            int execResult = -1;

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json", optional: true, reloadOnChange: false)
                .Build();

            var zipConfig = config.GetSection("zip");
            var assemblerConfig = config.GetSection("assembler");

            string outputfileAssembler = Path.Combine(o.RoomPath, fileName + ".bin");
            string inputFile = Path.Combine(o.RoomPath, outputFile);

            if (assemblerConfig != null)
            {
                string pathExe = assemblerConfig.GetSection("app").Value;
                string param = assemblerConfig.GetSection("args").Value;

                param = param.Replace("%1", inputFile).Replace("%2", outputfileAssembler);

                execResult = ExecuteCommand(pathExe, param);
            }

            if (zipConfig != null && execResult == 0)
            {
                string pathExe = zipConfig.GetSection("app").Value;
                string param = zipConfig.GetSection("args").Value;

                string outputfileCompress = Path.Combine(Path.GetDirectoryName(outputfileAssembler), Path.GetFileNameWithoutExtension(outputfileAssembler) + ".zx0");

                param = param.Replace("%1", outputfileAssembler).Replace("%2", outputfileCompress);

                execResult = ExecuteCommand(pathExe, param);
            }

            BuildList(o.RoomPath, "*.zx0", o.RoomPath + "\\list.txt");

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

            File.WriteAllText(outputFile, listOut.ToString());
        }
    }
}
