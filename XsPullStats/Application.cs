using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace XsPullStats
{
    public class Application
    {
        MdStatsEngine mdGenerator;

        public Application(string jsonFile)
        {
            String settings;
            StreamReader reader = new StreamReader(jsonFile);
            settings = reader.ReadToEnd();
            reader.Close();
            //
            this.mdGenerator = new MdStatsEngine(JsonConvert.DeserializeObject<GitSettings>(settings));
        }

        public void Start()
        {
            this.mdGenerator.Create();
        }
    }
}
