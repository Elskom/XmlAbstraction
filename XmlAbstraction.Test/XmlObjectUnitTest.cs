namespace XmlAbstraction.Test
{
    using System;
    using System.IO;
    using System.Text;
    using Xunit;

    public class XmlObjectUnitTest
    {
        private static void Throws(ref XmlObject xmlObj)
        {
            try
            {
                xmlObj.ReopenFile();
                throw new Exception("Method did not throw.");
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static void NoThrows(ref XmlObject xmlObj)
        {
            try
            {
                xmlObj.ReopenFile();
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Method threw an exception.");
            }
        }

        [Fact]
        public void TestClassReopenFile()
        {
            var testXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<test>
</test>";
            var xmlObj = new XmlObject(testXml);
            Throws(ref xmlObj);
            var fstrm = File.Create(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
            fstrm.Write(Encoding.UTF8.GetBytes(testXml), 0, testXml.Length);
            fstrm.Dispose();
            xmlObj.Dispose();
            xmlObj = new XmlObject(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml",
                testXml);
            NoThrows(ref xmlObj);
            xmlObj.Dispose();
            File.Delete(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
        }

        [Fact]
        public void TestClassDoubleDispose()
        {
            var testXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<test>
</test>";
            var xmlObj = new XmlObject(testXml);
            xmlObj.Dispose();
            xmlObj.Dispose();
        }
    }
}
