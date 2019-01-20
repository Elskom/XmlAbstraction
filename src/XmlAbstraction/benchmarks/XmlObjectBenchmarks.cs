namespace XmlAbstraction.Benchmark
{
    using System;
    using System.IO;
    using System.Text;
    using BenchmarkDotNet.Attributes;
    using XmlAbstraction;

    [ClrJob(baseline: true), CoreJob, MonoJob, CoreRtJob]
    [RPlotExporter, RankColumn]
    public class XmlObjectBenchmarks
    {
        private XmlObject xmlObj;
        private string XmlFile = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}BenchmarkTest.xml";
        
        [Params(@"<?xml version=""1.0"" encoding=""utf-8"" ?><test></test>", @"<?xml version=""1.0"" encoding=""utf-8"" ?><test><test1 TestAttribute1=""0"">test</test1><test2 TestAttribute1=""0"">test2</test2></test>")]
        public string InputXml;
        
        [GlobalSetup]
        public void Setup()
        {
            using (var fstrm = File.Create(XmlFile))
            {
                fstrm.Write(Encoding.UTF8.GetBytes(InputXml), 0, InputXml.Length);
            }

            xmlObj = new XmlObject(XmlFile, InputXml);
        }

        [Benchmark]
        public void ReopenFile()
            => xmlObj.ReopenFile();

        // TODO: Benchmark every method in the XmlAbstraction library.

        [Benchmark]
        public void Save()
            => xmlObj.Save();
    }
}
