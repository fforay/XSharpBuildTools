using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace XsBuildApp
{
    class Application
    {
        BuildSettings settingEnv;

        public Application( string jsonFile )
        {
            String settings;
            StreamReader reader = new StreamReader(jsonFile);
            settings = reader.ReadToEnd();
            reader.Close();
            //
            settingEnv = JsonConvert.DeserializeObject<BuildSettings>(settings);
        }

        public void Start()
        {
            // Register the Env Variables
            foreach( var keyValue in this.settingEnv.EnvVars)
            {
                Environment.SetEnvironmentVariable( keyValue.Key, keyValue.Value, EnvironmentVariableTarget.Process);
            }
            //
            //var globalProperty = new Dictionary<String, String>();
            //globalProperty.Add("Configuration", "Debug"); //<--- change here 
            //globalProperty.Add("Platform", "Any CPU");//<--- change here 
            var globalProperty = this.settingEnv.Props;
            //
            string projectFileName = this.settingEnv.Project; //"C:\\XSharp\\Dev\\XSharp\\Master.sln";//<--- change here can be another VS type ex: .vcxproj
            XSharpLogger Logger = new XSharpLogger();
            var projectCollection = new ProjectCollection();
            var buildParamters = new BuildParameters(projectCollection);
            buildParamters.Loggers = new List<Microsoft.Build.Framework.ILogger>() { Logger };

            BuildManager.DefaultBuildManager.ResetCaches();
            var buildRequest = new BuildRequestData(projectFileName, globalProperty, null, new String[] { "Build" }, null);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("Build Started");

            var buildResult = BuildManager.DefaultBuildManager.Build(buildParamters, buildRequest);
            if (buildResult.OverallResult == BuildResultCode.Failure)
            {
                // catch result ..
            }

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            //Console.WriteLine(Logger.GetLogString());    //display output ..
            Console.WriteLine("Processed {0} Projects.", Logger.Projects.Count);
            Console.WriteLine("{0} Projects with Errors.", Logger.Errors.Count);
            foreach( var PrjWithErrors in Logger.Errors )
            {
                Console.WriteLine("Project : " + PrjWithErrors.Key);
                foreach( var Error in PrjWithErrors.Value )
                {
                    String[] errorInfo = Error.Split(',');
                    Console.WriteLine("     Error : File {0}; Line {1}; Column {2}", errorInfo[0], errorInfo[1], errorInfo[2]);
                }
            }

            Console.WriteLine("{0} Projects with Warnings.", Logger.Warnings.Count);
            foreach (var PrjWithWarnings in Logger.Warnings)
            {
                Console.WriteLine("Project : " + PrjWithWarnings.Key);
                foreach (var Error in PrjWithWarnings.Value)
                {
                    String[] warningInfo = Error.Split(',');
                    Console.WriteLine("     Warning : File {0}; Line {1}; Column {2}", warningInfo[0], warningInfo[1], warningInfo[2]);
                }
            }

        }
    }
}
