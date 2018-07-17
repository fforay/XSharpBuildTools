using LibGit2Sharp;
using LibGit2Sharp.Handlers;
//using HeyRed.MarkdownSharp;
using MarkdownLog;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsPullStats
{
    class Program
    {
        public static void Main(String[] args)
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
                Console.WriteLine("XsPullStats <toBuild>");
                Console.WriteLine("<toBuild> is a .json file that will contain information about the build.");
                Console.WriteLine("");
            }
        }
    }
}
