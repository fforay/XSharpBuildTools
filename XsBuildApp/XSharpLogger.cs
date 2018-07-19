using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace XsBuildApp
{
    public class XSharpLogger : Logger
    {
        public List<String> Projects;
        public Dictionary<String, List<String>> Errors;
        public Dictionary<String, List<String>> Warnings;
        object objLock = new object();


        /// <summary>
        /// Initialize is guaranteed to be called by MSBuild at the start of the build
        /// before any events are raised.
        /// </summary>
        public override void Initialize(IEventSource eventSource)
        {

            //
            Projects = new List<string>();
            Errors = new Dictionary<string, List<string>>();
            Warnings = new Dictionary<string, List<string>>();

            // For brevity, we'll only register for certain event types. Loggers can also
            // register to handle TargetStarted/Finished and other events.
            eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
            eventSource.WarningRaised += new BuildWarningEventHandler(eventSource_WarningRaised);
            eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            lock (objLock)
            {
                //
                String Error = String.Format("{0},{1},{2}", e.File, e.LineNumber, e.ColumnNumber);
                String file = Path.GetFileNameWithoutExtension(e.ProjectFile);
                List<String> errList;
                if (!Errors.ContainsKey(file))
                {
                    errList = new List<String>();
                    errList.Add(Error);

                    Errors.Add(file, errList);
                }
                else
                {
                    errList = Errors[file];
                    errList.Add(Error);
                }
            }
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            lock (objLock)
            {
                //
                String Warning = String.Format("{0},{1},{2}", e.File, e.LineNumber, e.ColumnNumber);
                String file = Path.GetFileNameWithoutExtension(e.ProjectFile);
                List<String> warnList;
                if (!Warnings.ContainsKey(file))
                {
                    warnList = new List<String>();
                    warnList.Add(Warning);

                    Warnings.Add(file, warnList);
                }
                else
                {
                    warnList = Warnings[file];
                    warnList.Add(Warning);
                }
            }
        }


        void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            lock (objLock)
            {
                String ext = Path.GetExtension(e.ProjectFile);
                if (String.Compare(".sln", ext, true) == 0)
                    return;
                String file = Path.GetFileNameWithoutExtension(e.ProjectFile);
                if (!Projects.Contains(file))
                {
                    Projects.Add(file);
                }
            }
        }


        /// <summary>
        /// Shutdown() is guaranteed to be called by MSBuild at the end of the build, after all 
        /// events have been raised.
        /// </summary>
        /// 
        /// 
        public override void Shutdown()
        {
        }
    }
}
