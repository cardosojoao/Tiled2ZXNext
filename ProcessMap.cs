using Microsoft.Extensions.Primitives;
using System.IO;
using System.Text;
using Tiled2ZXNext.Extensions;

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
            string fileName = tiledData.Properties.GetProperty( "FileName");

            result.Append('m');
            result.Append(fileName);
            result.Append(":\r\n");
            result.Append(".Left:\t\t");
            result.Append("db\t");
            result.Append(tiledData.Properties.GetProperty( "roomleft"));
            result.Append("\r\n");
            result.Append(".Right:\t\t");
            result.Append("db\t");
            result.Append(tiledData.Properties.GetProperty( "roomright"));
            result.Append("\r\n");
            result.Append(".Top:\t\t");
            result.Append("db\t");
            result.Append(tiledData.Properties.GetProperty( "roomtop"));
            result.Append("\r\n");
            result.Append(".Bottom:\t");
            result.Append("db\t");
            result.Append(tiledData.Properties.GetProperty( "roombottom"));
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
