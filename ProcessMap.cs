using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tiled2ZXNext
{
    public partial class Controller
    {

        /// <summary>
        /// Creat the room details for the map
        /// </summary>
        /// <param name="tiledData"></param>
        /// <returns></returns>
        public StringBuilder ProcessMap(TiledParser tiledData)
        {
            StringBuilder result = new StringBuilder(100);
            string fileName = TiledParser.GetProperty(tiledData.Properties, "FileName");

            result.Append('m');
            result.Append(fileName);
            result.Append(":\r\n");
            result.Append(".Left:\t\t");
            result.Append("db\t");
            result.Append(TiledParser.GetProperty(tiledData.Properties, "roomleft"));
            result.Append("\r\n");
            result.Append(".Right:\t\t");
            result.Append("db\t");
            result.Append(TiledParser.GetProperty(tiledData.Properties, "roomright"));
            result.Append("\r\n");
            result.Append(".Top:\t\t");
            result.Append("db\t");
            result.Append(TiledParser.GetProperty(tiledData.Properties, "roomtop"));
            result.Append("\r\n");
            result.Append(".Bottom:\t");
            result.Append("db\t");
            result.Append(TiledParser.GetProperty(tiledData.Properties, "roombottom"));
            result.Append("\r\n");
            return result;
        }


        public void OutputMap(Options o, StringBuilder mapData)
        {
            string pathOutput = Path.Combine(o.MapPath, outputFile);
            File.WriteAllText(pathOutput, mapData.ToString());
        }
    }
}
