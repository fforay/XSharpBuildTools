using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsBuildApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string toBuild = null;
            if ( args.Length >= 1 )
            {
                toBuild = args[0];
                toBuild = Path.ChangeExtension(toBuild, "json");
            }
            //
            if (File.Exists(toBuild))
            {
                Application app = new Application( toBuild );
                app.Start();
            }
            else
            {
                Console.WriteLine("Usage :");
                Console.WriteLine("XsBuildApp <toBuild>");
                Console.WriteLine("<toBuild> is a .json file that will contain information about the build.");
                Console.WriteLine("");
            }
        }
    }
}
