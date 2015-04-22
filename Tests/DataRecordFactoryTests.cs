using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTRider.DynamicDataRecord;

namespace Tests
{
    [TestClass]
    public class DataRecordFactoryTests
    {
        [TestMethod]
        public void EndToEnd()
        {
            var dr = new TestDataRecord()
                .Add("name", "name value")
                .Add("description", "description value")
                .Add("count", 123)
                .Add("timestamp", new DateTime(1972,01,03));

            dynamic record = DataRecordFactory.BindRecord("Record", dr);

            Assert.AreEqual(record.name, "name value");
            Assert.AreEqual(record.description, "description value");
            Assert.AreEqual(record.count, 123);
            Assert.AreEqual(record.timestamp, new DateTime(1972, 01, 03));

        }


        [TestMethod]
        public void Serialization()
        {
            var dr = new TestDataRecord()
                .Add("name", "name value")
                .Add("description", "description value")
                .Add("count", 123)
                .Add("timestamp", new DateTime(1972, 01, 03));

            var binder = DataRecordFactory.GetRecordBinder("Record", dr);
            var serializer = new XmlSerializer(binder.Type);

            dynamic record = binder.Bind(dr);

            Assert.AreEqual(record.name, "name value");
            Assert.AreEqual(record.description, "description value");
            Assert.AreEqual(record.count, 123);
            Assert.AreEqual(record.timestamp, new DateTime(1972, 01, 03));

            var writer = new StringWriter();
            serializer.Serialize(writer, record);

            var value = writer.ToString();
            Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<Record xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <name>name value</name>
  <description>description value</description>
  <count>123</count>
  <timestamp>1972-01-03T00:00:00</timestamp>
</Record>",value);



        }


        [TestMethod]
        public void NameFix()
        {
            var dr = new TestDataRecord()
                .Add("some name", "name value")
                .Add("123description", "description value")
                .Add("co!@#unt", 123);

            dynamic record = DataRecordFactory.BindRecord("Record2", dr);

            Assert.AreEqual(record.some_name, "name value");
            Assert.AreEqual(record._123description, "description value");
            Assert.AreEqual(record.co___unt, 123);
        }
    }
}
