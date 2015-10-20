using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Beazley.AutomationFramework.Configuration
{
    public class PageObjectConfiguration : ConfigurationElement
    {
        protected internal Dictionary<string, object> Values { get; set; }

        public PageObjectConfiguration()
        {
            Values = new Dictionary<string, object>();
        }

        [ConfigurationProperty("name", IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("basePageName", IsRequired = false)]
        public string BasePageName
        {
            get
            {
                return (string)this["basePageName"];
            }
            set
            {
                this["basePageName"] = value;
            }
        }

        [ConfigurationProperty("urlPageName", IsRequired = false)]
        public string UrlPageName
        {
            get
            {
                return (string)this["urlPageName"];
            }
            set
            {
                this["urlPageName"] = value;
            }
        }

        [TypeConverter(typeof(TypeNameConverter))]
        [ConfigurationProperty("type", IsRequired = true)]
        public Type Type
        {
            get
            {
                return (Type)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        [ConfigurationProperty("url")]
        public Uri Url
        {
            get
            {
                return (Uri)this["url"];
            }
            set
            {
                this["url"] = value;
            }
        }

        [ConfigurationProperty("individual")]
        public bool Individual
        {
            get
            {
                return (bool)this["individual"];
            }
            set
            {
                this["individual"] = value;
            }
        }

        [ConfigurationProperty("Elements", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(ElementConfiguration), AddItemName = "Element")]
        public ElementConfigurationCollection Elements
        {
            get
            {
                return (ElementConfigurationCollection)this["Elements"];
            }
            set
            {
                this["Elements"] = value;
            }
        }


        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            try
            {
                object destinationValue;
                PropertyInfo pi = Type.GetProperty(elementName);
                if (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string))
                {
                    string value = reader.ReadElementContentAsString();
                    destinationValue = Convert.ChangeType(value, pi.PropertyType);
                }
                else
                {
                    XmlSerializer ser = new XmlSerializer(pi.PropertyType);
                    destinationValue = ser.Deserialize(reader);
                }
                Values.Add(elementName, destinationValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
