using System.Configuration;

namespace Beazley.AutomationFramework.Configuration
{
    public class PageObjectConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PageObjectConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as PageObjectConfiguration).Type;
        }

        public void Add(PageObjectConfiguration pageConfig)
        {
            BaseAdd(pageConfig);
        }
    }
}
