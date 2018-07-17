using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsPullStats
{
    /// <summary>
    /// Information definition for each file
    /// </summary>
    public class FileData
    {
        public string Path { get; internal set; }
        public int linesAdded { get; internal set; }
        public int linesDeleted { get; internal set; }
        public string status { get; internal set; }
    }
}
