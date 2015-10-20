using System;
using System.Configuration;
using Beazley.AutomationFramework.Enums;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.Configuration
{
    public class ElementConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
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

        [ConfigurationProperty("by", DefaultValue = FindBy.Name)]
        public FindBy FindBy
        {
            get
            {
                return (FindBy)this["by"];
            }
            set
            {
                this["by"] = value;
            }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return (string)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }

        [ConfigurationProperty("isParameterized", IsRequired = false, DefaultValue = false)]
        public bool IsParameterized
        {
            get
            {
                return (bool)this["isParameterized"];
            }
            set
            {
                this["isParameterized"] = value;
            }
        }

        public Func<string, IWebElement> Finder { get; set; }

        public Func<string, string[], IWebElement> ParameterizedFinder { get; set; }

        public IWebElement FindElement(params string[] args)
        {
            if (IsParameterized)
            {
                return ParameterizedFinder(Name, args);
            }
            return Finder(Name);
        }
    }
}
