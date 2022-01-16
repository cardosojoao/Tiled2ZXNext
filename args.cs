using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Tiled2ZXNext
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input file")]
        public string Input { get; set; }

        [Option('z', "compress", Required = false, HelpText = "Compress file", Default = false)]
        public bool Compress { get; set; }

        [Option('r', "roompath", Required = true, HelpText = "Room output path")]
        public string RoomPath{ get; set; }

        [Option('m', "mappath", Required = true, HelpText = "map output path")]
        public string MapPath{ get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }
}
