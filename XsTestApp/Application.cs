using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Runners;

namespace XsTestApp
{
    class Application
    {
        TestAppSettings settingEnv;

        // We use consoleLock because messages can arrive in parallel, so we want to make sure we get
        // consistent console output.
        object consoleLock = new object();
        // Use an event to know when we're done
        ManualResetEvent finished = new ManualResetEvent(false);

        // List of Failed Tests
        List<String> failed;
        // List of Passed Tests
        List<String> passed;

        public Application(string jsonFile)
        {
            String settings;
            StreamReader reader = new StreamReader(jsonFile);
            settings = reader.ReadToEnd();
            reader.Close();
            //
            settingEnv = JsonConvert.DeserializeObject<TestAppSettings>(settings);
        }

        public void Start()
        {
            //
            foreach (String assembly in settingEnv.Assemblies)
            {
                // The next Assembly to test is
                string testAssembly = Path.Combine(settingEnv.Path, assembly);
                if (File.Exists(testAssembly))
                {
                    finished.Reset();
                    // Create lists
                    failed = new List<string>();
                    passed = new List<string>();

                    using (var runner = AssemblyRunner.WithAppDomain(testAssembly))
                    {
                        runner.OnExecutionComplete = OnExecutionComplete;
                        runner.OnTestFailed = OnTestFailed;
                        runner.OnTestPassed = OnTestPassed;

                        Console.WriteLine("Running...");
                        runner.Start();

                        finished.WaitOne();
                        // Now generate Result file
                        // ....
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Assembly not Found : {0}", testAssembly);
                    Console.ResetColor();
                }
            }

            finished.Dispose();
        }

        void OnTestPassed(TestPassedInfo info)
        {
            lock (consoleLock)
                Console.WriteLine("[Passed] {0} / {1}", info.TestDisplayName, info.MethodName);
            //
            passed.Add(info.TestDisplayName);
        }

        void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (consoleLock)
                Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
            //Raise Event
            finished.Set();
        }

        void OnTestFailed(TestFailedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
                if (info.ExceptionStackTrace != null)
                    Console.WriteLine(info.ExceptionStackTrace);

                Console.ResetColor();
            }
            //
            failed.Add(info.TestDisplayName);
        }

    }
}
