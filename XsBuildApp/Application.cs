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

using MarkdownLog;

namespace XsBuildApp
{
    class Application
    {
        BuildSettings settingEnv;
        List<String> projects = new List<string>();
        List<String> projectsWithErrors = new List<string>();
        List<String> projectsWithWarnings = new List<string>();

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
            if (!File.Exists(this.settingEnv.Project))
            {
                if (this.settingEnv.Console)
                {
                    Console.WriteLine("Project file not found : {0}.", settingEnv.Project);
                }
                return;
            }
            //
            if (this.settingEnv.NewMDFile)
            {
                if (File.Exists(this.settingEnv.MDFile))
                {
                    File.Delete(this.settingEnv.MDFile);
                }
            }
            //
            // Register the Env Variables
            foreach ( var keyValue in this.settingEnv.EnvVars)
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
            XSharpLogger Logger = new XSharpLogger( );
            var projectCollection = new ProjectCollection();
            var buildParamters = new BuildParameters(projectCollection);
            buildParamters.Loggers = new List<Microsoft.Build.Framework.ILogger>() { Logger };

            BuildManager.DefaultBuildManager.ResetCaches();
            var buildRequest = new BuildRequestData(projectFileName, globalProperty, null, new String[] { "Build" }, null);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            if (this.settingEnv.Console)
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
            if (this.settingEnv.Console)
                Console.WriteLine("RunTime " + elapsedTime);

            //Console.WriteLine(Logger.GetLogString());    //display output ..
            foreach (var PrjWithErrors in Logger.Errors)
            {
                this.projectsWithErrors.Add(PrjWithErrors.Key);
            }
            foreach (var PrjWithWarnings in Logger.Warnings)
            {
                this.projectsWithWarnings.Add(PrjWithWarnings.Key);
            }
            //
            if (this.settingEnv.Console)
            {
                Console.WriteLine("Processed {0} Projects.", Logger.Projects.Count);
                Console.WriteLine("{0} Projects with Errors.", Logger.Errors.Count);
                foreach (var PrjWithErrors in Logger.Errors)
                {
                    Console.WriteLine("Project : " + PrjWithErrors.Key);
                    this.projectsWithErrors.Add(PrjWithErrors.Key);
                    foreach (var Error in PrjWithErrors.Value)
                    {
                        String[] errorInfo = Error.Split(',');
                        Console.WriteLine("     Error : File {0}; Line {1}; Column {2}", errorInfo[0], errorInfo[1], errorInfo[2]);
                    }
                }

                Console.WriteLine("{0} Projects with Warnings.", Logger.Warnings.Count);
                foreach (var PrjWithWarnings in Logger.Warnings)
                {
                    Console.WriteLine("Project : " + PrjWithWarnings.Key);
                    this.projectsWithWarnings.Add(PrjWithWarnings.Key);
                    foreach (var Error in PrjWithWarnings.Value)
                    {
                        String[] warningInfo = Error.Split(',');
                        Console.WriteLine("     Warning : File {0}; Line {1}; Column {2}", warningInfo[0], warningInfo[1], warningInfo[2]);
                    }
                }
            }
            this.projects = Logger.Projects;
            // Now, produce a MD file with the results
            // ...
            generateMD(this.settingEnv.Project);
        }

        private void generateMD(string project)
        {
            //Creating the list of (not)building project
            List<String> buildingProjects = new List<String>();
            List<String> notBuildingProjects = new List<String>();
            foreach (var proj in this.projects)
            {
                if ( (!this.projectsWithErrors.Contains(proj)) || (this.settingEnv.WarningAsError && !this.projectsWithWarnings.Contains(proj)) )
                    buildingProjects.Add(proj);
                else
                    notBuildingProjects.Add(proj);

            }
            // Create a list of result
            List<BuildData> testData = new List<BuildData>();
            foreach (var testOk in buildingProjects)
            {
                testData.Add(new BuildData() { ProjectName = testOk, Result = "![Success](" + this.settingEnv.Success+ ")" });
            }
            foreach (var testBad in notBuildingProjects)
            {
                testData.Add(new BuildData() { ProjectName = testBad, Result = "![Failure](" + this.settingEnv.Failure + ")" });
            }
            // Sort by TestName
            testData.Sort((test1, test2) => test1.ProjectName.CompareTo(test2.ProjectName));
            // Now, open/create the file
            FileStream md = new FileStream(this.settingEnv.MDFile, FileMode.OpenOrCreate);
            // and write
            StreamWriter sr = new StreamWriter(md);
            // Move to the end of File, in order to add results
            md.Seek(0, SeekOrigin.End);
            sr.WriteLine("");
            sr.WriteLine("");
            sr.WriteLine("");
            String[] hourWithMili = DateTime.Now.TimeOfDay.ToString().Split('.');
            // Set the Time as a Paragraph Header
            String header = Path.GetFileName(project) + " " + hourWithMili[0];
            var headerMD = header.ToMarkdownHeader();
            sr.WriteLine(headerMD);
            //
            Table tbl = testData.ToMarkdownTable();
            // Hack to remove the first five spaces on each line (or GitHub wrongly shows the table)
            StringReader read = new StringReader(tbl.ToMarkdown());
            do
            {
                String line = read.ReadLine();
                if (line != null)
                {
                    if (line.Length >= 5)
                        sr.WriteLine(line.Substring(5));
                    else
                        sr.WriteLine(line);
                }
                else
                    break;
            } while (true);
            //
            read.Close();
            sr.Close();
            md.Close();

        }
    }
}
