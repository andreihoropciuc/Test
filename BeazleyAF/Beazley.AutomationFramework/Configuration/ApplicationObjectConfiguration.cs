using System.Configuration;

namespace Beazley.AutomationFramework.Configuration
{
    public class ApplicationObjectConfiguration : ConfigurationSection
    {
        #region Singleton Instance

        readonly static string SECTION_NAME = "Application";

        static readonly object Sync = new object();
        static volatile ApplicationObjectConfiguration _instance;

        public static ApplicationObjectConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Sync)
                    {
                        if (_instance == null)
                        {
                            _instance = (ApplicationObjectConfiguration)ConfigurationManager.GetSection(SECTION_NAME);
                        }
                    }
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        #endregion

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

        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(PageObjectConfiguration), AddItemName = "Page")]
        public PageObjectConfigurationCollection Pages
        {
            get
            {
                return (PageObjectConfigurationCollection)this[""];
            }
            set
            {
                this[""] = value;
            }
        }
    }
}
