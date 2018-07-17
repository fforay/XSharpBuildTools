using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsBuildApp
{
    class BuildSettings
    {
        // String with a list Env Vars
        // Var,Value;Var,Value
        public string Env { get; set; }

        /// <summary>
        /// Dictionnary of Environnement Variables and their value to Set before running the Build
        /// </summary>
        public Dictionary<string, string> EnvVars
        {
            get
            {
                Dictionary<string, string> dict;
                if ( !String.IsNullOrEmpty(this.Env) )
                    dict = this.DecodeDictionnary(this.Env);
                else
                    dict = new Dictionary<string, string>();
                return dict;
            }
        }

        public string Properties { get; set; }

        /// <summary>
        /// Dictionnary of Build Property with their values to set before running the Build
        /// </summary>
        public Dictionary<string, string> Props
        {
            get
            {
                Dictionary<string, string> dict;
                if (!String.IsNullOrEmpty(this.Properties))
                    dict = this.DecodeDictionnary(this.Properties);
                else
                    dict = new Dictionary<string, string>();
                return dict;
            }
        }

        /// <summary>
        /// The FullPath Solution file to build
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Flag indicating if we must produce some Console informations
        /// </summary>
        public bool Console { get; set; }

        private Dictionary<string, string>  DecodeDictionnary( string value )
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            String[] firstLevel = value.Split(';');
            foreach (string block in firstLevel)
            {
                String[] keyValue = block.Split(',');
                if (keyValue.Length == 2)
                    dict.Add(keyValue[0], keyValue[1]);
            }
            return dict;
        }

    }
}
