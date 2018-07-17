namespace XsTestApp
{
    public class TestData_Link
    {
        public string TestName { get; internal set; }
        public string Result { get; internal set; }
    }

    public class TestData
    {
        public string TestName { get; internal set; }
        public bool Result { get; internal set; }
    }

    public class TestData_Vagrant
    {
        public string TestAssembly { get; internal set; }
        public string Result { get; internal set; }
    }
}