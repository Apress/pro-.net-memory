using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Microsoft.IO;

namespace Benchmarks.Tests
{
    [CoreJob]
    [MemoryDiagnoser]
    public class RecyclableMemoryStreamTest
    {
        private RecyclableMemoryStreamManager manager;
        private XmlWriterSettings xmlWriterSettings;
        private object obj;

        [GlobalSetup]
        public void Setup()
        {
            xmlWriterSettings = new XmlWriterSettings();
            manager = new RecyclableMemoryStreamManager(blockSize: 128 * 1024,
                largeBufferMultiple: 1024 * 1024,
                maximumBufferSize: 128 * 1024 * 1024);
            obj = new SomeDataClass() { X = 1, Y = 2 };
        }

        [Benchmark]
        public string SerializeXmlWithMemoryStream()
        {
            using (var ms = new MemoryStream())
            {
                using (var xw = XmlWriter.Create(ms, xmlWriterSettings))
                {
                    var serializer = new DataContractSerializer(obj.GetType()); // could be cached!
                    serializer.WriteObject(xw, obj);
                    xw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(ms);
                    return reader.ReadToEnd();
                }
            }
        }

        [Benchmark]
        public string SerializeXmlWithRecyclableMemoryStream()
        {
            using (var ms = manager.GetStream())
            {
                using (var xw = XmlWriter.Create(ms, xmlWriterSettings))
                {
                    var serializer = new DataContractSerializer(obj.GetType());
                    serializer.WriteObject(xw, obj);
                    xw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(ms);
                    return reader.ReadToEnd();
                }
            }
        }
    }

    public class SomeDataClass
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
