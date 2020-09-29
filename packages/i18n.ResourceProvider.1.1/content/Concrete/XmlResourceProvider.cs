using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Resources.Abstract;
using Resources.Entities;

namespace Resources.Concrete
{
    public class XmlResourceProvider: BaseResourceProvider
    {
        // File path
        private static string filePath = null;
        

        public XmlResourceProvider(){}
        public XmlResourceProvider(string filePath)
        {            
            XmlResourceProvider.filePath = filePath;

            if (!File.Exists(filePath)) throw new FileNotFoundException(string.Format("XML Resource file {0} was not found", filePath));
        }

        protected override IList<ResourceEntry> ReadResources()
        {
           
            // Parse the XML file
            return XDocument.Parse(File.ReadAllText(filePath))
                .Element("resources")
                .Elements("resource")
                .Select(e => new ResourceEntry {
                    Name = e.Attribute("name").Value,
                    Value = e.Attribute("value").Value,
                    Culture = e.Attribute("culture").Value                    
                }).ToList();
        }

        protected override ResourceEntry ReadResource(string name, string culture)
        {
            // Parse the XML file
            return XDocument.Parse(File.ReadAllText(filePath))
                .Element("resources")
                .Elements("resource")
                .Where(e => e.Attribute("name").Value == name && e.Attribute("culture").Value == culture)
                .Select(e => new ResourceEntry {
                    Name = e.Attribute("name").Value,
                    Value = e.Attribute("value").Value,
                    Culture = e.Attribute("culture").Value
                }).FirstOrDefault();
        }
    }
}
