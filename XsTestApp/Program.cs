using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace XsTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string toBuild = null;
            if (args.Length >= 1)
            {
                toBuild = args[0];
                toBuild = Path.ChangeExtension(toBuild, "json");
            }
            //
            if (File.Exists(toBuild))
            {
                Application app = new Application(toBuild);
                app.Start();
            }
            else
            {
                Console.WriteLine("Usage :");
                Console.WriteLine("XsTestApp <toTest>");
                Console.WriteLine("<toTest> is a .json file that will contain information about the tests.");
                Console.WriteLine("");
            }
        }

    }
}

