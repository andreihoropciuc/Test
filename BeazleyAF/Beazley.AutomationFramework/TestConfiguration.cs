using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Beazley.AutomationFramework
{
    public interface ITestConfiguration
    {
        IDictionary<string, string> EntryPoints { get; }
    }

    public class TestConfiguration : ITestConfiguration
    {
        private readonly XDocument _configuration;
        public TestConfiguration(string filePath)
        {
            _configuration = XDocument.Load(filePath, LoadOptions.None);
        }

        public IDictionary<string, string> EntryPoints
        {
            get
            {
                try
                {
                    return (
                                from xmlNode in _configuration.Descendants("entryPoint")
                                select new KeyValuePair<string, string>(GetAttributeValue(xmlNode, "typeName"), GetAttributeValue(xmlNode, "url"))
                            ).ToDictionary(x => x.Key, y => y.Value);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message + " " + e.StackTrace);
                }

                return null;
            }
        }

        private string GetAttributeValue(XElement xmlNode, string attributeName)
        {
            return xmlNode.Attribute(XName.Get(attributeName, string.Empty)).Value;
        }

        public static string GetPageUrl(string pageName)
        {
            return ConfigurationManager.AppSettings[pageName];
        }

        public static string ApplicationUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ApplicationUrl"];
            }
        }
    }
}
