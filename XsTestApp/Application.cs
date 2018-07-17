using MarkdownLog;
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

                        if (settingEnv.Console)
                            Console.WriteLine("Running...");
                        runner.Start();

                        finished.WaitOne();
                        // Now generate Result file
                        if (settingEnv.Vagrant)
                            this.generateMD_Vagrant(testAssembly);
                        else
                            this.generateMD(testAssembly);
                        // ....
                    }
                }
                else
                {
                    if (settingEnv.Console)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Assembly not Found : {0}", testAssembly);
                        Console.ResetColor();
                    }
                }
            }

            finished.Dispose();
        }


        void OnTestPassed(TestPassedInfo info)
        {
            if (settingEnv.Console)
                lock (consoleLock)
                    Console.WriteLine("[Passed] {0} / {1}", info.TestDisplayName, info.MethodName);
            //
            passed.Add(info.TestDisplayName);
        }

        void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            if (settingEnv.Console)
                lock (consoleLock)
                    Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
            //Raise Event
            finished.Set();
        }

        void OnTestFailed(TestFailedInfo info)
        {
            if (settingEnv.Console)
            {
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
                    if (info.ExceptionStackTrace != null)
                        Console.WriteLine(info.ExceptionStackTrace);

                    Console.ResetColor();
                }
            }
            //
            failed.Add(info.TestDisplayName);
        }


        private void generateMD(string testAssembly)
        {
            // Create a list of result
            List<TestData> testData = new List<TestData>();
            foreach (var testOk in passed)
            {
                testData.Add(new TestData() { TestName = testOk, Result = true });
            }
            foreach (var testBad in failed)
            {
                testData.Add(new TestData() { TestName = testBad, Result = false });
            }
            // Sort by TestName
            testData.Sort((test1, test2) => test1.TestName.CompareTo(test2.TestName));
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
            String header = Path.GetFileName(testAssembly) + " " + hourWithMili[0];
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

        private bool headerAdded = false;

        private void generateMD_Vagrant(string testAssembly)
        {
            // Create a list of result
            List<TestData_Vagrant> testData_Vagrant = new List<TestData_Vagrant>();
            TestData_Vagrant vagrant = new TestData_Vagrant();
            vagrant.TestAssembly = Path.GetFileName(testAssembly);
            vagrant.Result = "";

            List<TestData> testData = new List<TestData>();
            foreach (var testOk in passed)
            {
                testData.Add(new TestData() { TestName = testOk, Result = true });
            }
            foreach (var testBad in failed)
            {
                testData.Add(new TestData() { TestName = testBad, Result = false });
            }
            // Sort by TestName
            testData.Sort((test1, test2) => test1.TestName.CompareTo(test2.TestName));
            //
            foreach (var td in testData)
            {
                if (td.Result)
                    vagrant.Result += ".";
                else
                    vagrant.Result += "F";
            }

            // Now, open/create the file
            FileStream md = new FileStream(this.settingEnv.MDFile, FileMode.OpenOrCreate);
            // and write
            StreamWriter sr = new StreamWriter(md);
            // Move to the end of File, in order to add results
            md.Seek(0, SeekOrigin.End);
            if (!headerAdded)
            {
                sr.WriteLine("");
                sr.WriteLine("");
                sr.WriteLine("");
                String[] hourWithMili = DateTime.Now.TimeOfDay.ToString().Split('.');
                // Set the Time as a Paragraph Header
                String header = hourWithMili[0];
                var headerMD = header.ToMarkdownHeader();
                sr.WriteLine(headerMD);
            }
            //
            testData_Vagrant.Add(vagrant);
            Table tbl = testData_Vagrant.ToMarkdownTable();
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
