namespace XmlAbstraction.Test
{
    using System;
    using System.IO;
    using System.Text;
    using Xunit;

    public class XmlObjectUnitTest
    {
        private static void NoThrows(Action expression)
        {
            try
            {
                expression();
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Expression threw an exception.");
            }
        }

        [Fact]
        public void TestClassReopenFile()
        {
            var testXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<test>
</test>";
            var xmlObj = new XmlObject(testXml);
            Assert.ThrowsAny<Exception>(() => xmlObj.ReopenFile());
            var fstrm = File.Create(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
            fstrm.Write(Encoding.UTF8.GetBytes(testXml), 0, testXml.Length);
            fstrm.Dispose();
            xmlObj.Dispose();
            xmlObj = new XmlObject(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml",
                testXml);
            NoThrows(() => xmlObj.ReopenFile());
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

        [Fact]
        public void TestClassDisposedExceptions()
        {
            var testXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<test>
</test>";
            var xmlObj = new XmlObject(testXml);
            xmlObj.Dispose();

            // test to make sure that ObjectDisposedException is thrown.
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.AddAttribute("test4", "test", "test"));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Write("test", "test"));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Write("test2", "test", "test"));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Write("test3", "test31", new string[] { "test1", "test2", "test3" }));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Read("test"));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Read("test2", "test"));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Read("test3", "test31", null));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Delete("test"));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Delete("test2", "test"));
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.ReopenFile());
            Assert.ThrowsAny<ObjectDisposedException>(() => xmlObj.Save());
        }

        [Fact]
        public void TestClassEdits()
        {
            var testXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<test>
</test>";
            var testXmlNoRoot = @"<test>
</test>";
            var xmlObj = new XmlObject(testXml);

            // test to make sure that InvalidOperationException is thrown.
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.AddAttribute("test4", "test", "test"));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Write("test", "test"));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Write("test2", "test", "test"));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Write("test3", "test31", new string[] { "test1", "test2", "test3" }));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Read("test"));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Read("test2", "test"));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Read("test3", "test31", null));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Delete("test"));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.Delete("test2", "test"));
            Assert.ThrowsAny<InvalidOperationException>(() => xmlObj.ReopenFile());
            xmlObj.Dispose();
            xmlObj = new XmlObject(testXmlNoRoot);
            // reopen data from a file.
            xmlObj.Dispose();
            var fstrm = File.Create(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
            fstrm.Write(Encoding.UTF8.GetBytes(testXml), 0, testXml.Length);
            fstrm.Dispose();
            xmlObj.Dispose();
            xmlObj = new XmlObject(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml",
                testXml);
            NoThrows(() => xmlObj.AddAttribute("test4", "test", "test"));
            NoThrows(() => xmlObj.Write("test", "test"));
            NoThrows(() => xmlObj.Write("test2", "test", "test"));
            NoThrows(() => xmlObj.Write("test3", "test", new string[] { "test1", "test2", "test3" }));
            NoThrows(() => xmlObj.Read("test"));
            NoThrows(() => xmlObj.Read("test2", "test"));
            NoThrows(() => xmlObj.Read("test3", "test", null));
            NoThrows(() => xmlObj.ReopenFile());
            NoThrows(() => xmlObj.Read("test"));
            NoThrows(() => xmlObj.Read("test2", "test"));
            NoThrows(() => xmlObj.Read("test3", "test", null));
            NoThrows(() => xmlObj.Delete("test"));
            NoThrows(() => xmlObj.Delete("test2", "test"));
            NoThrows(() => xmlObj.Save());
            xmlObj.Dispose();
            File.Delete(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
            xmlObj = new XmlObject($"{Path.DirectorySeparatorChar}test.xml", testXml, true);
            xmlObj.Dispose();
            xmlObj = new XmlObject($"{Path.DirectorySeparatorChar}test.xml", testXml, true);
            xmlObj.Dispose();
            xmlObj = new XmlObject($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml", testXml, true);
            xmlObj.Dispose();
        }
    }
}
