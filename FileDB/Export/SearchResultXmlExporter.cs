using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FileDB.Export
{
    public class SearchResultXmlExporter : ISearchResultExporter
    {
        public void Export(SearchResultFileFormat data, string filename)
        {
            var xmlSerializer = new XmlSerializer(data.GetType());
            using var xmlFileStream = new StreamWriter(filename);
            using var xmlWriter = XmlWriter.Create(xmlFileStream, new XmlWriterSettings { Indent = true });
            xmlSerializer.Serialize(xmlWriter, data);
        }
    }
}
