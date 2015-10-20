using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Beazley.AutomationFramework.Configuration;
using Beazley.AutomationFramework.Contexts;
using Beazley.AutomationFramework.Enums;
using Beazley.AutomationFramework.Selenium;
using log4net;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    public class BasePage
    {        
        public IWebDriver Driver { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public static TimeSpan DefaultTimeout { get; set; }
        public static TimeSpan DefaultPollingFrequency { get; set; }

        public BasePage(IWebDriver driver, string title, string url, ContainerWebElement parentContainer = null)
        {
            GetPageFromConfig();

            Driver = driver;
            Title = title;

            var containerBasePage = this as ContainerBasePage;
            if (containerBasePage != null) containerBasePage.Container = parentContainer;

            DefaultTimeout = TimeSpan.FromSeconds(30);
            DefaultPollingFrequency = TimeSpan.FromMilliseconds(250);
            Url = url;

            Load();

            StoreWindowHandle();
        }        
        
        protected static ILog Log = LogManager.GetLogger(typeof(BasePage));

        readonly Dictionary<string, ElementConfiguration> _cachedQueries = new Dictionary<string, ElementConfiguration>(StringComparer.InvariantCultureIgnoreCase);

        public string LastKnownWindowHandle { get; set; }

        public ReadOnlyCollection<string> WindowHandlesSnapshot { get; set; }

        private string TitleToWaitFor
        {
            get
            {
                if (this is ContainerBasePage)
                {
                    BasePage currentPage = this;
                    var container = currentPage as ContainerBasePage;

                    while (container != null)
                    {
                        currentPage = container.ParentPage;
                        container = currentPage as ContainerBasePage;
                    }

                    return currentPage.Title;
                }
                return Title;
            }
        }

        public bool IsLoaded
        {
            get
            {
                if (TitleToWaitFor == null)
                    throw new Exception("PageObject has no Title specified");

                string pageTitle = null;

                try
                {
                    pageTitle = Driver.Title;
                }
                catch (Exception e)
                {
                    Log.Error("Page Title could not be read!", e);
                }

                var isLoaded = pageTitle != null && pageTitle.ToLowerInvariant().Contains(TitleToWaitFor.ToLowerInvariant());

                Log.Info(string.Format("IsLoaded = {0}, pageTitle = {1} contains {2}", isLoaded, pageTitle ?? "(null)", TitleToWaitFor ?? "(null)"));

                return isLoaded;
            }
        }

        public IWebElement Find(string name, params string[] args)
        {
            IWebElement elem = null;
            if (_cachedQueries.ContainsKey(name))
            {
                ElementConfiguration elemConfig = _cachedQueries[name];
                elem = elemConfig.FindElement(args);
            }
            return elem;
        }    

        public IAlert Alert
        {
            get
            {
                try
                {
                    return Driver.SwitchTo().Alert();
                }
                catch (NoAlertPresentException nape)
                {
                    Log.Warn("Alert not present.", nape);
                }
                return null;
            }
        }

        public bool IsAlertPresent
        {
            get
            {
                try
                {
                    Driver.SwitchTo().Alert();
                    return true;
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            }
        }        

        protected static string WaitForAlertAsLongAs(Func<bool> waitCondition, Func<string> doingSomething = null, int customTimeToWaitForAlert = -1)
        {
            var timeToWaitForAlert = new TimeSpan(0, 0, 0, 0, customTimeToWaitForAlert);
            var sw = new Stopwatch();
            string result = null;
            sw.Start();
            while (waitCondition())
            {
                Log.Info("WaitForAlertAsLongAs(): start loop");
                if (sw.Elapsed > Globals.ImplicitWait || (customTimeToWaitForAlert > 0 && sw.Elapsed > timeToWaitForAlert))
                {
                    Log.Info(string.Format("WaitForAlertAsLongAs(): Waiting for condition timed Out after {0}. The expected condition was not met!", Globals.ImplicitWait));
                    break;
                }
                if (doingSomething != null)
                {
                    result = doingSomething();
                    Log.Info(string.Format("WaitForAlertAsLongAs(): doingSomething = {0}", result ?? "(null)"));
                }
                Thread.Sleep(100);
            }
            Log.Info("WaitForAlertAsLongAs(): exit loop");
            sw.Stop();
            return result;
        }
               
        #region Private methods

        private void StoreWindowHandle()
        {
            LastKnownWindowHandle = Driver.CurrentWindowHandle;
            WindowHandlesSnapshot = Driver.WindowHandles;
        }

        // Gets the page from config file together with its properties
        private void GetPageFromConfig()
        {
            PageObjectConfiguration currentPage = ApplicationObjectConfiguration.Instance.Pages.OfType<PageObjectConfiguration>().FirstOrDefault(p => p.Type == this.GetType());
            if (currentPage != null)
            {
                if (currentPage.Individual)
                {
                    Url = TestConfiguration.GetPageUrl(string.IsNullOrEmpty(currentPage.UrlPageName)
                        ? currentPage.Name
                        : currentPage.UrlPageName) + (currentPage.Url != null ? currentPage.Url.OriginalString : "");
                }
                else
                {

                    Url = TestConfiguration.ApplicationUrl + (currentPage.Url != null ? currentPage.Url.OriginalString : "");
                }

                ParsePageConfig(currentPage);
            }
        }

        private IWebElement FindElementBy(string query, FindBy findBy = FindBy.Name)
        {
            try
            {
                switch (findBy)
                {
                    case FindBy.Id:
                        return Driver.FindElement(By.Id(query));
                    case FindBy.Name:
                        return Driver.FindElement(By.Name(query));
                    case FindBy.XPath:
                        return Driver.FindElement(By.XPath(query));
                    case FindBy.Href:
                        return Driver.FindElement(By.XPath(string.Format("//*[@href='{0}']", query)));
                    case FindBy.Value:
                        return Driver.FindElement(By.XPath(string.Format("//*[@value='{0}']", query)));
                    case FindBy.LinkText:
                        return Driver.FindElement(By.LinkText(query));
                    case FindBy.PartialLinkText:
                        return Driver.FindElement(By.PartialLinkText(query));
                    case FindBy.ClassName:
                        return Driver.FindElement(By.ClassName(query));
                    default:
                        throw new NotSupportedException();
                }
            }
            catch (Exception exc)
            {
                Log.Info(string.Format("Could not find element with {0} = '{1}'", findBy, query));                
            }

            return null;
        }

        private IWebElement ParameterizedFindElementBy(string paramQuery, FindBy findBy, params string[] args)
        {
            string query = string.Format(paramQuery, args);
            return FindElementBy(query, findBy);
        }

        private PageObjectConfiguration GetBasePage(PageObjectConfiguration pageObjectConfig)
        {
            PageObjectConfiguration result = null;

            if (!string.IsNullOrEmpty(pageObjectConfig.BasePageName))
            {
                result = ApplicationObjectConfiguration.Instance.Pages.OfType<PageObjectConfiguration>().FirstOrDefault(p => p.Name == pageObjectConfig.BasePageName);
            }

            return result;
        }

        private void ParsePageConfig(PageObjectConfiguration pageObjectConfig)
        {
            PageObjectConfiguration basePage = GetBasePage(pageObjectConfig);
            if (basePage != null)
            {
                ParsePageConfig(basePage);
            }

            foreach (ElementConfiguration ec in pageObjectConfig.Elements)
            {
                string value = ec.Value;
                FindBy findBy = ec.FindBy;

                if (ec.IsParameterized)
                {
                    Func<string, string[], IWebElement> parameterizedFinder = (str, args) => ParameterizedFindElementBy(value, findBy, args);
                    ec.ParameterizedFinder = parameterizedFinder;
                }
                else
                {
                    Func<string, IWebElement> finder = (str) => FindElementBy(value, findBy);
                    ec.Finder = finder;
                }

                _cachedQueries.Add(ec.Name, ec);
            }

            foreach (var kp in pageObjectConfig.Values)
            {
                pageObjectConfig.Type.GetProperty(kp.Key).SetValue(this, kp.Value, null);
            }
        }

        private object GetPendingAjaxCallsCount(IJavaScriptExecutor jsExecutor, object result)
        {
            try
            {
                result = jsExecutor.ExecuteScript(
                    @"
					  if (typeof jQuery != 'undefined') {  
					  // jQuery is loaded  
					  return jQuery.active;
					  } else {
					  // jQuery is not loaded
					  return -1;
					  }");
            }
            catch (Exception e)
            {
                Log.Info("Error: " + e.Message);
            }
            return result;
        }

        #endregion

        #region Public Methods       

        public static void SetWaitDefaults(TimeSpan timeout, TimeSpan pollingFrequency)
        {
            DefaultTimeout = timeout;
            DefaultPollingFrequency = pollingFrequency;
        }

        public void Load()
        {
            try
            {
                var benchmarkDescription = string.Format("Page Load(): Loading Page [{0}] - [{1}] - [{2}] ", GetType().FullName, Title, Url ?? "(null)");

                using (new BenchmarkContext(benchmarkDescription))
                {
                    if (!IsLoaded)
                    {
                        if (!string.IsNullOrWhiteSpace(Url))
                        {
                            Driver.Navigate().GoToUrl(Url);
                        }
                        else
                        {
                            Log.Info("Page Load(): Navigation was performed via navigation element. (Not Url Address) [Benchmark not accurate]");
                        }

                        Log.Info("Page Load(): <<<<< Started Navigation Guard");
                        var sw = new Stopwatch();
                        var retriesLeft = 3;
                        sw.Start();
                        while (!IsLoaded && retriesLeft >= 0)
                        {
                            if (sw.Elapsed > TimeSpan.FromSeconds(30))
                            {
                                Type thisPage = GetType();

                                Log.Warn(string.Format("Page Load(): <<<  NOTE: Navigation Guard Thread RE-TRIGGERING navigation for {0} !  retriesLeft = {1} >>>", thisPage, retriesLeft));

                                Globals.TriggerNavigationFor(thisPage);
                                sw.Reset();
                                sw.Start();
                                retriesLeft--;
                            }
                            Log.Info("Page Load(): ... waiting ...");
                            Thread.Sleep(100);
                        }
                        sw.Stop();
                        Log.Info("Page Load(): Ended Navigation Guard >>>>>");
                    }


                    WaitForPageToLoad();
                }
            }
            catch (Exception e)
            {
                Log.Error("Page Load(): Error while loading page! ", e);
                throw;
            }
        }        

        public static void TakeScreenshot(IWebDriver driver, string identifier)
        {
            string filename = null;
            try
            {
                if (ConfigurationManager.AppSettings["ScreenshotOutput"] == null || ConfigurationManager.AppSettings["ScreenshotOutput"] == string.Empty)
                {
                    Log.Info("Could not save screenshot brcause ScreenshotOutput not defined");
                }
                else
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    var now = DateTime.Now;
                    string appPath = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin\\", StringComparison.Ordinal));
                    string screenshotsOutput = ConfigurationManager.AppSettings["ScreenshotOutput"];
                    var folder = new DirectoryInfo(string.Format("{0}\\{1}\\{2}", appPath, screenshotsOutput, identifier));
                    if (!folder.Exists)
                    {
                        folder.Create();
                    }

                    filename = string.Format("{0}{1}\\{2}\\{3}_{3:00}_{4:00}_{5:0000}_{6:00}_{7:00}_{8:00}_{9:000}.png",
                        appPath, screenshotsOutput, identifier, now.Day, now.Month, now.Year,
                        now.Hour, now.Minute, now.Second, now.Millisecond);
                                        
                    screenshot.SaveAsFile(filename, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception e)
            {
                Log.Error(string.Format("Could not save Screenshot -> {0}", filename), e);
            }
        }

        /// <summary>
        ///  Waits for the dynamic content on page to load: updater overlay to dissapear (depends on custom wait implementation), pending ajax calls to finish.
        /// </summary>
        public void WaitForDynamicContentToLoad()
        {
            if (!IsAlertPresent)	//this wasn't reliable for Click() so a special method has been put in place. If this appears to be the case for other methods treat separately
            {                
                WaitForPendingAjaxCalls();
            }
        }

        public void WaitForPendingAjaxCalls()
        {
            Log.Info("WaitForPendingAjaxCalls(): Start");

            var jsExecutor = (Driver as IJavaScriptExecutor);

            if (jsExecutor == null)
                return;

            bool ajaxCallsPending = true;


            while (ajaxCallsPending) // Handle timeout somewhere
            {
                object result = null;

                result = GetPendingAjaxCallsCount(jsExecutor, result);

                if (result == null)
                {
                    Log.Info("WaitForPendingAjaxCalls(): JavaScript Executor Error!");
                    break;
                }

                int numberOfPendingAjaxCalls = Convert.ToInt32(result);

                Log.Info(string.Format("WaitForPendingAjaxCalls(): Waiting for pending Ajax calls. Active = {0}", numberOfPendingAjaxCalls));

                ajaxCallsPending = numberOfPendingAjaxCalls > 0;
            }

            Log.Info("WaitForPendingAjaxCalls(): Finished ajax calls wait...");

        }

        public void WaitForTitleChange(string title)
        {
            if (Title != null)
            {
                string titleToWaitFor = title ?? TitleToWaitFor;

                Log.Info(string.Format("WaitForTitleChange(): Starting wait for _title update from {0} to {1}", Driver.Title,
                    titleToWaitFor));

                Until.Title.Contains(titleToWaitFor).Wait();

                Log.Info(string.Format("WaitForTitleChange(): Finished _title update from {0} to {1}", Driver.Title, titleToWaitFor));
            }
        }

        /// <summary>
        /// Waits for the page to fully load: _title to change, pade to have a body, updater overlay to dissapear (depends on custom wait implementation), pending ajax calls to finish.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="testAppearance"></param>
        public void WaitForPageToLoad(string title = null, bool testAppearance = true)
        {
            WaitForTitleChange(title);
            //if (testAppearance)
            //CustomPageLoadWait_TestAppearance();
            //CustomPageLoadWait_TestDissapearance();
            //WaitForPendingAjaxCalls();
        }

        #region [Action Methods]

        /// <summary>
        /// Also waits for parent page to fully load (_title to change, updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <returns></returns>
        public string AcceptAlert(bool waitForPageToLoadAfter = true)
        {
            return ActOnAlert(AlertAction.Accept, waitForPageToLoadAfter);
        }

        /// <summary>
        /// Also waits for parent page to fully load (_title to change, updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <returns></returns>
        public string DismissAlert(bool waitForPageToLoadAfter = true)
        {
            return ActOnAlert(AlertAction.Dismiss, waitForPageToLoadAfter);
        }

        public void WaitForAlert(int timeToWaitForAlert = -1)
        {
            Log.Info("WaitForAlert(): Waiting for alert to appear...");
            WaitForAlertAsLongAs(() => !IsAlertPresent, null, timeToWaitForAlert);
            Log.Info("WaitForAlert(): Waiting for alert ended.");
        }

        /// <summary>
        /// Also waits for parent page to fully load (_title to change, updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <param name="alertAction"></param>
        /// <param name="waitForPageToLoadAfter"></param>
        /// <returns></returns>
        public string ActOnAlert(AlertAction alertAction, bool waitForPageToLoadAfter = true)
        {
            Log.Info(string.Format("{0}ing alert in {1}", alertAction, Title));
            string alertText = WaitForAlertAsLongAs(() => IsAlertPresent, () =>
            {
                var alert = Driver.SwitchTo().Alert();
                alertText = alert.Text;

                Log.Info(string.Format("{0}ing Alert with TEXT = {1}", alertAction, alertText));
                switch (alertAction)
                {
                    case AlertAction.Accept:
                        alert.Accept();
                        break;
                    case AlertAction.Dismiss:
                        alert.Dismiss();
                        break;
                    default:
                        alert.Dismiss();
                        break;
                }

                if (waitForPageToLoadAfter)
                    WaitForPageToLoad();
                return alertText;
            });

            Log.Info(string.Format("Alert {0}ed in {1}", alertAction, Title));

            if (waitForPageToLoadAfter)
                WaitForPageToLoad();
            return alertText;
        }

        /// <summary>
        /// Also waits for page to fully load (title to change, updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        public void Refresh(bool waitForPageToLoad = true)
        {
            ExecuteScript("window.location.reload();");

            if (waitForPageToLoad)
                WaitForPageToLoad();
        }

        public void ExecuteScript(string jsCode)
        {
            var jsExecutor = (Driver as IJavaScriptExecutor);

            if (jsExecutor != null)
                jsExecutor.ExecuteScript(jsCode);
        }

        #endregion

        #endregion

    }
}
