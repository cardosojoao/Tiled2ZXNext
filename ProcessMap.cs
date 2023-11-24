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
        /// <param name="scene"></param>
        /// <returns></returns>
        public StringBuilder ProcessMap(Entities.Scene scene)
        {
            StringBuilder result = new StringBuilder(100);
            string fileName = scene.Properties.GetProperty( "FileName");
            result.Append('m');
            result.Append(fileName);
            result.Append(":\r\n");
            result.Append(".Left:\t\tdb\t").Append(scene.Properties.GetProperty( "roomleft")).Append("\r\n");
            result.Append(".Right:\t\tdb\t").Append(scene.Properties.GetProperty( "roomright")).Append("\r\n");
            result.Append(".Top:\t\tdb\t").Append(scene.Properties.GetProperty( "roomtop")).Append("\r\n");
            result.Append(".Bottom:\tdb\t").Append(scene.Properties.GetProperty( "roombottom")).Append("\r\n");
            return result;
        }

        public void OutputMap(Options o, StringBuilder mapData)
        {
            string pathOutput = Path.Combine(o.MapPath, outputFile);
            File.WriteAllText(pathOutput, mapData.ToString());
        }
    }
}
