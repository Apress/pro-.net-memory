using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MemoryLeaks.Leaks
{
    class XmlSerializerLeak : IMemoryLeakExample
    {
        public void Run()
        {
            var rand = new Random();
            while (true)
            {
                XMLObj obj = new XMLObj() {Nodes = new List<XMLNode>() {new XMLNode() {Value = rand.Next(int.MaxValue)} }};
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLObj), new XmlRootAttribute("rootNode"));
                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, obj);
                    Console.WriteLine(textWriter.ToString());
                }
                Thread.Sleep(100);
            }
        }
    }

    [Serializable]
    public class XMLObj
    {
        [XmlElement("block")]
        public List<XMLNode> Nodes { get; set; }
    }

    [Serializable]
    public class XMLNode
    {
        public int Value { get; set; }
    }
}
