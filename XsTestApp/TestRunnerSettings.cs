using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsTestApp
{
    class TestAppSettings
    {
        public bool Console { get; set; }

        public string Path { get; set; }

        public List<String> Assemblies { get; set; }

        public string MDFile { get; set; }

        public bool Details { get; set; }

        public bool NewMDFile { get; set; }

        public string Passed { get; set; }

        public string Failed { get; set; }

    }
}
