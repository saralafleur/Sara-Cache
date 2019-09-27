using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sara.NETStandard.Cache
{
    /// <summary>
    /// Adds the ability to a List of Interfaces to serialize and deserialize - Sara
    /// </summary>
    /// <remarks>
    /// The following code comes from an example from the following page
    /// http://www.codeproject.com/Articles/738100/XmlSerializer-Serializing-list-of-interfaces
    /// </remarks>
    public class ListOfICacheData : List<ICacheData>, IXmlSerializable
    {
        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("ListOfICacheData");

            if (reader.EOF)
                return;

            while (reader.IsStartElement("ICacheData"))
            {
                // ReSharper disable AssignNullToNotNullAttribute
                var type = Type.GetType(reader.GetAttribute("AssemblyQualifiedName"));
                if (type == null)
                {
                    reader.ReadToNextSibling("ICacheData");
                    continue;
                }

                var serial = new XmlSerializer(type);
                // ReSharper restore AssignNullToNotNullAttribute

                reader.ReadStartElement("ICacheData");

                // ReSharper disable once PossibleNullReferenceException
                Add((ICacheData)serial.Deserialize(reader));
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var data in this)
            {
                writer.WriteStartElement("ICacheData");
                // ReSharper disable AssignNullToNotNullAttribute
                writer.WriteAttributeString("AssemblyQualifiedName", data.GetType().AssemblyQualifiedName);
                // ReSharper restore AssignNullToNotNullAttribute
                var xmlSerializer = new XmlSerializer(data.GetType());
                xmlSerializer.Serialize(writer, data);
                writer.WriteEndElement();
            }
        }
    }
}
