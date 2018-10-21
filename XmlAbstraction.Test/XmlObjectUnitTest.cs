﻿namespace XmlAbstraction.Test
{
    using System;
    using System.IO;
    using System.Text;
    using Xunit;

    public class XmlObjectUnitTest
    {
        private const string testXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?><test></test>";

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
            var xmlObj = new XmlObject(testXml);
            Assert.ThrowsAny<Exception>(() => xmlObj.ReopenFile());
            var fstrm = File.Create(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
            fstrm.Write(Encoding.UTF8.GetBytes(testXml), 0, testXml.Length);
            fstrm.Dispose();
            xmlObj = new XmlObject(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml",
                testXml);
            NoThrows(() => xmlObj.ReopenFile());
            File.Delete(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
        }

        [Fact]
        public void TestClassEdits()
        {
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
            xmlObj = new XmlObject(testXmlNoRoot);
            // reopen data from a file.
            var fstrm = File.Create(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
            fstrm.Write(Encoding.UTF8.GetBytes(testXml), 0, testXml.Length);
            fstrm.Dispose();
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
            NoThrows(() => xmlObj.Read("test4"));
            NoThrows(() => xmlObj.ReopenFile());
            NoThrows(() => xmlObj.Write("test", "testnew"));
            NoThrows(() => xmlObj.Read("test"));
            NoThrows(() => xmlObj.Read("test2", "test"));
            NoThrows(() => xmlObj.Read("test3", "test", null));
            NoThrows(() => xmlObj.Delete("test"));
            NoThrows(() => xmlObj.Delete("test2", "test"));
            NoThrows(() => xmlObj.Save());
            File.Delete(
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml");
            xmlObj = new XmlObject($"{Path.DirectorySeparatorChar}test.xml", testXml, true);
            xmlObj = new XmlObject($"{Path.DirectorySeparatorChar}test.xml", testXml, true);
            xmlObj = new XmlObject($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}test.xml", testXml, true);
        }

        [Fact]
        public void Test_contructor_root_missing_Fail()
        {
            Assert.Throws<System.Xml.XmlException>(() => new XmlObject(""));
        }

        [Fact]
        public void Test_create_file_current_directory_Pass()
        {
            var testXmlFile = @"testCreate.xml";

            if (File.Exists($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile))
                File.Delete($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var xmlObj = new XmlObject(testXmlFile, testXml, true);
            xmlObj.Save();

            Assert.True(File.Exists(testXmlFile));
        }

        [Fact]
        public void Test_create_file_remote_directory_Pass()
        {
            // test with a real directory
            var testXmlFile = @"C:\Temp\testCreate.xml";

            if (File.Exists(testXmlFile))
                File.Delete(testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var xmlObj = new XmlObject(testXmlFile, testXml, true);
            xmlObj.Save();

            Assert.True(File.Exists(testXmlFile));
        }

        [Fact]
        public void Test_create_file_remote_violation_Fail()
        {
            // test with a real directory that doesnt have write access
            var testXmlFile = @"C:\testCreate.xml";

            if (File.Exists(testXmlFile))
                File.Delete(testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var xmlObj = new XmlObject(testXmlFile, testXml, true);

            Assert.Throws<UnauthorizedAccessException>(() => xmlObj.Save());
            Assert.False(File.Exists(testXmlFile));
        }

        [Fact]
        public void Test_create_file_remote_not_found_Fail()
        {
            // test with a real directory that doesnt have write access
            var testXmlFile = @"C:\nothere\testCreate.xml";

            if (File.Exists(testXmlFile))
                File.Delete(testXmlFile);

            Assert.False(File.Exists(testXmlFile));
            Assert.Throws<DirectoryNotFoundException>(() => new XmlObject(testXmlFile, testXml, true));
        }

        [Fact]
        public void Test_add_attribute_Pass()
        {
            var testXmlFile = @"testAddAttribute.xml";

            if (File.Exists($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile))
                File.Delete($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var element = "TestElement";
            var attribute = "TestAttribute";
            var attributeValue = "my cool value";

            var xmlObj = new XmlObject(testXmlFile, testXml, true);
            xmlObj.AddAttribute(element, attribute, attributeValue);
            xmlObj.Save();
            xmlObj.ReopenFile();

            var result = xmlObj.Read(element, attribute);
            Assert.Equal(result, attributeValue);
        }

        // Should be able to add attribute and update that attribute if it hasnt been saved
        [Fact]
        public void Test_add_update_attribute_Pass()
        {
            var testXmlFile = @"testAddAttribute.xml";

            if (File.Exists($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile))
                File.Delete($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var element = "TestElement";
            var attribute = "TestAttribute";
            var attributeValue = "my cool value";
            var newAttributeValue = "my new cool value";

            var xmlObj = new XmlObject(testXmlFile, testXml, true);
            xmlObj.AddAttribute(element, attribute, attributeValue);
            xmlObj.AddAttribute(element, attribute, newAttributeValue);
            xmlObj.Save();
            xmlObj.ReopenFile();

            var result = xmlObj.Read(element, attribute);
            Assert.Equal(result, newAttributeValue);
        }

        // Should not be able to update attribute that exists if the file has been loaded/saved
        [Fact]
        public void Test_update_attribute_Fail()
        {
            var testXmlFile = @"testAddAttribute.xml";

            if (File.Exists($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile))
                File.Delete($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var element = "TestElement";
            var attribute = "TestAttribute";
            var attributeValue = "my cool value";
            var newAttributeValue = "my new cool value";

            var xmlObj = new XmlObject(testXmlFile, testXml, true);
            xmlObj.AddAttribute(element, attribute, attributeValue);
            xmlObj.Save();
            Assert.Throws<Exception>(() => xmlObj.AddAttribute(element, attribute, newAttributeValue));
        }

        [Fact]
        public void Test_add_element_Pass()
        {
            var testXmlFile = @"testAddElement.xml";

            if (File.Exists($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile))
                File.Delete($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var element = "TestElement";
            var elementValue = "element value";

            var xmlObj = new XmlObject(testXmlFile, testXml, true);
            xmlObj.Write(element, elementValue);
            xmlObj.Save();
            xmlObj.ReopenFile();

            var result = xmlObj.Read(element);
            Assert.Equal(result, elementValue);
        }

        [Fact]
        public void Test_delete_file_element_Pass()
        {
            var testXmlFile = @"testDelElement.xml";

            if (File.Exists($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile))
                File.Delete($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}" + testXmlFile);

            Assert.False(File.Exists(testXmlFile));

            var element = "TestElement";
            var elementValue = "element value";

            var xmlObj = new XmlObject(testXmlFile, testXml, true);
            xmlObj.Write(element, elementValue);
            xmlObj.Save();
            xmlObj.ReopenFile();
            xmlObj.Delete(element);
            xmlObj.Save();
            xmlObj.ReopenFile();

            var result = xmlObj.Read(element);
            Assert.NotEqual(result, element);
        }
    }
}
