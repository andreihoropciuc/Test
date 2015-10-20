using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Beazley.AutomationFramework.InfrastructureObjects;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;

namespace Beazley.AutomationFramework.Selenium
{
    public class BeazleyWebDriverTestBase
    {
        protected static ILog Log = LogManager.GetLogger(typeof(BeazleyWebDriverTestBase));
        protected static IWebDriver Driver;
        private static TimeSpan _implicitWaitDefault;

        private static TestConfiguration _testConfiguration;
        private static string _log4NetConfigFilePath;        

        static BeazleyWebDriverTestBase()
        {            
            LoadTestConfiguration();

            Globals.Setup(_testConfiguration);
            Website.Initialize(_testConfiguration.EntryPoints);
        }

        #region Public methods

        [TestFixtureSetUp]
        public void RunBeforeAllTests()
        {
            ListAppSettings();

            OverrideLoggerPathAccordingToAppDomainConfig();

            TraceLoggersInfo();
            try
            {
                InitializeBrowser();

                BeforeAll();
            }
            catch (Exception e)
            {
                Log.Error("Error When Initialising Test Fixture", e);

                if (TestContextHasFullName())
                {
                    BasePage.TakeScreenshot(Driver, "TFSetupFail_" + TestContext.CurrentContext.Test.FullName);
                }

                QuitDriver();

                throw;
            }
        }

        [TestFixtureTearDown]
        public void RunAfterAllTests()
        {
            try
            {
                AfterAll();
            }
            catch (Exception e)
            {
                Log.Error("Error when running AfterAll(): ", e);
            }

            Log.Warn("Closing Driver!");

            QuitDriver();
        }

        [SetUp]
        public void RunBeforeEachTest()
        {
            Log.Info("# Starting Test : " + TestContext.CurrentContext.Test.FullName);
            BeforeEach();
        }

        [TearDown]
        public void RunAfterEachTest()
        {
            try
            {
                if (TestFailed())
                {
                    BasePage.TakeScreenshot(Driver, "TestFail_" + TestContext.CurrentContext.Test.FullName);
                }

                Log.Info("# Finished Test : " + TestContext.CurrentContext.Test.FullName);
            }
            catch
            {
            }
            AfterEach();
        }

        public static TestConfiguration Configuration
        {
            get { return _testConfiguration; }
        }

        public void ListAppSettings()
        {
            Log.Info(string.Format("Config Keys Count === {0}", ConfigurationManager.AppSettings.Count));
            Log.Info(string.Format("ApplicationBase = {0}", AppDomain.CurrentDomain.SetupInformation.ApplicationBase));
            Log.Info(string.Format("ConfigurationFile = {0}", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));            

            foreach (string key in ConfigurationManager.AppSettings.Keys)
            {
                Log.Info(string.Format("appSetting key={0} value={1}", key, ConfigurationManager.AppSettings[key] ?? "(null)"));
            }
        }

        protected static void InitializeBrowser()
        {
            switch (ConfigurationManager.AppSettings["BrowserDriver"])
            {
                case "iexplore":
                    {
                        Driver = new InternetExplorerDriver(ReadBrowserDriversPathFromConfig());
                    }
                    break;
                case "chrome":
                    {
                        Driver = new ChromeDriver(ReadBrowserDriversPathFromConfig());
                    }
                    break;
                case "firefox":
                    {
                        Driver = new FirefoxDriver();
                    }
                    break;
                case "safari":
                    {
                        Driver = new SafariDriver();
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException(string.Format("BrowserDriver={0}", ConfigurationManager.AppSettings["BrowserDriver"] ?? "(null)"));
            }

            _implicitWaitDefault = ReadImplicitWaitTimeoutFromConfig();

            Globals.Setup(Driver, _implicitWaitDefault);

            Driver.Manage().Timeouts().SetPageLoadTimeout(ReadPageLoadTimeoutFromConfig());

            Driver.Manage().Timeouts().ImplicitlyWait(_implicitWaitDefault);

            BasePage.SetWaitDefaults(ReadExplicitWaitDefaultTimeoutFromConfig(), ReadExplicitWaitDefaultPollingFrequencyFromConfig());

            Log.Info("Loaded Driver!");

            SetUpCulture();
        }                

        public virtual void BeforeEach()
        {
        }

        public virtual void AfterEach()
        {
        }

        public virtual void BeforeAll()
        {
        }

        public virtual void AfterAll()
        {
        }


        #endregion

        #region Private methods

        private static void LoadTestConfiguration()
        {
            Log.Info("Searching beazleyConfiguration.config...");
            string configFilePath = FindConfigFile(AppDomain.CurrentDomain.BaseDirectory);

            Log.Info(string.IsNullOrWhiteSpace(configFilePath)
                                ? "Not Found."
                                : string.Format("Found: {0}", configFilePath));

            _testConfiguration = new TestConfiguration(configFilePath);

            _log4NetConfigFilePath = BuildFilePath(GetParentFolderPath(configFilePath), ConfigurationManager.AppSettings["Log4NetConfigurationFilePath"]);
            Log.Info(string.Format("log4net combined path = {0}", _log4NetConfigFilePath));
        }

        private static string BuildFilePath(string rootFolder, string relativePath)
        {
            return Path.Combine(rootFolder, relativePath);
        }

        private static string GetParentFolderPath(string startingFolder)
        {
            return startingFolder.Remove(startingFolder.LastIndexOf('\\'));
        }

        private static string FindConfigFile(string startingFolder)
        {
            Trace.WriteLine(startingFolder);
            if (startingFolder.Length < 3)
                return null;

            string searchResult = Directory.EnumerateFiles(startingFolder, "*.*", SearchOption.TopDirectoryOnly).FirstOrDefault(x => x.ToLowerInvariant().EndsWith("beazley.config"));

            return string.IsNullOrWhiteSpace(searchResult)
                ? FindConfigFile(GetParentFolderPath(startingFolder))
                : searchResult;
        }

        private static bool TestContextHasResult()
        {
            return TestContext.CurrentContext.Result != null;
        }

        private static bool TestFailed()
        {
            if (!TestContextHasResult())
            {
                return false;
            }

            if (TestContext.CurrentContext.Result.Status == TestStatus.Failed)
            {
                return true;
            }

            return false;
        }

        private static bool TestContextHasFullName()
        {
            return !string.IsNullOrEmpty(TestContext.CurrentContext.Test.FullName);
        }

        private static void OverrideLoggerPathAccordingToAppDomainConfig()
        {
            try
            {
                var log4NetConfig = new FileInfo(_log4NetConfigFilePath);

                XmlConfigurator.ConfigureAndWatch(log4NetConfig);

                var h = (Hierarchy)LogManager.GetRepository();

                if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["LogFileOutput"]))
                {
                    Log.Info("No LogFileOutput defined. Skipping FileAppender output path override...");
                    return;
                }

                foreach (FileAppender a in h.Root.Appenders.OfType<FileAppender>())
                {
                    OverrideAppenderFileOutput(a);
                    break;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("ERROR ACCESSING RESOURCE!" + e.Message);
            }
        }

        private static void OverrideAppenderFileOutput(IAppender a)
        {
            var fa = (FileAppender)a;

            string outputLogPath = BuildLogFolderPathFromConfig(fa);

            if (!string.IsNullOrWhiteSpace(outputLogPath))
            {
                fa.File = outputLogPath;

                fa.ActivateOptions();
            }

            Trace.WriteLine(string.Format("[Final Value] {0} -> fa.File = {1}", fa.Name, fa.File));
        }

        private static string BuildLogFolderPathFromConfig(FileAppender fa)
        {
            string logFileOutput = ConfigurationManager.AppSettings["LogFileOutput"];

            if (string.IsNullOrWhiteSpace(logFileOutput))
                return null;

            return logFileOutput;
        }

        private static void SetUpCulture()
        {
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            culture.DateTimeFormat.LongDatePattern = "dddd, MMMM dd, yyyy";
            culture.DateTimeFormat.ShortTimePattern = "HH:mm";
            culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            Thread.CurrentThread.CurrentCulture = culture;
        }

        private static void TraceLoggersInfo()
        {
            ILog[] loggers = LogManager.GetCurrentLoggers();

            if (loggers == null)
                return;

            foreach (ILog lgr in loggers)
            {
                Trace.WriteLine("t1t>" + lgr.Logger.Name);
                Trace.WriteLine(string.Format(">> {0} - D={1}, I={2}, W={3}, E={4}, F={5} ", lgr.Logger.Name, lgr.IsDebugEnabled,
                                              lgr.IsInfoEnabled, lgr.IsWarnEnabled, lgr.IsErrorEnabled, lgr.IsFatalEnabled));
            }
        }

        private static TimeSpan ReadExplicitWaitDefaultPollingFrequencyFromConfig()
        {
            return TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["ExplicitWaitDefaultPollingFrequency"] ?? "250"));
        }

        private static TimeSpan ReadPageLoadTimeoutFromConfig()
        {
            return TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["PageLoadTimeout"] ?? "60000"));
        }

        private static TimeSpan ReadImplicitWaitTimeoutFromConfig()
        {
            return TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["ImplicitWaitTimeout"] ?? "60000"));
        }

        private static string ReadBrowserDriversPathFromConfig()
        {
            return ReadPathTagFromConfig("BrowserDriversPath");
        }

        private static TimeSpan ReadExplicitWaitDefaultTimeoutFromConfig()
        {
            return TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["ExplicitWaitDefaultTimeout"] ?? "20000"));
        }

        private static string ReadPathTagFromConfig(string tagName)
        {
            var pathFromConfigurationFile = ConfigurationManager.AppSettings[tagName];

            if (pathFromConfigurationFile.StartsWith("\\"))
                return (AppDomain.CurrentDomain.BaseDirectory + pathFromConfigurationFile);

            if (File.Exists(pathFromConfigurationFile))
                return pathFromConfigurationFile;

            throw new Exception(String.Format("Driver for selected browser not found at given path {0}", pathFromConfigurationFile));
        }

        #endregion

        

        

        protected static void QuitDriver()
        {
            if (Driver != null)
                Driver.Quit();
        }
              
    }
}
