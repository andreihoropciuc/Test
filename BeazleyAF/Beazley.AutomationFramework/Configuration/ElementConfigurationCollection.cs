using System.Configuration;

namespace Beazley.AutomationFramework.Configuration
{
    public class ElementConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ElementConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ElementConfiguration).Name;
        }
    }
}
