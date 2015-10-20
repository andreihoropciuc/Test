using System;
using Beazley.AutomationFramework.InfrastructureObjects;
using Beazley.AutomationFramework.Selenium;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Beazley.AutomationFramework
{
    public class WaitHelper
    {
        public enum WaitTypes
        {
            ElementExists,
            ElementNotExist,
            ElementIsVisible,
            ElementIsNotVisible,
            TitleContains,
            TitleIs,
            ElementIsEnabled,
            ElementIsNotStale,
            TextNotPresent,
            TextPresent,
            ElementIsNotEnabled,
            ElementIsVisibleAndEnabled
        }

        protected TimeSpan? Timeout;
        protected TimeSpan? PollingFrequency;

        protected WaitTypes WaitType = WaitTypes.ElementExists;
        protected string TextToWait;

        protected bool IsElement;

        protected WaitHelper(bool isElement)
        {
            IsElement = isElement;
        }
    }


    public class Step3WaitHelper : WaitHelper
    {
        protected readonly ILog Log = LogManager.GetLogger(typeof(Step3WaitHelper));
        public Step3WaitHelper(bool isElement)
            : base(isElement)
        { }

        public void Wait(string timeoutMessage = null)
        {
            if (IsElement)
            {
                var waitHelper = (WaitElementHelper)this;
                var webDriverWait = new WebDriverWait(waitHelper.WebElement.ParentPage.Driver, Timeout ?? Globals.ImplicitWait);

                //webDriverWait.IgnoreExceptionTypes(typeof (StaleElementReferenceException));

                if (!string.IsNullOrWhiteSpace(timeoutMessage))
                    webDriverWait.Message = timeoutMessage;
                webDriverWait.PollingInterval = PollingFrequency ?? BasePage.DefaultPollingFrequency;

                switch (WaitType)
                {
                    case WaitTypes.ElementIsVisible:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_ElementIsVisible(waitHelper.WebElement));
                        break;
                    case WaitTypes.ElementIsNotVisible:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_ElementIsNotVisible(waitHelper.WebElement));
                        break;
                    case WaitTypes.ElementIsEnabled:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_ElementIsEnabled(waitHelper.WebElement));
                        break;
                    case WaitTypes.ElementIsNotEnabled:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_ElementIsEnabled(waitHelper.WebElement));
                        break;
                    case WaitTypes.ElementIsNotStale:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_IsNotStale(waitHelper.WebElement));
                        break;
                    case WaitTypes.ElementExists:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_ElementExists(waitHelper.WebElement));
                        break;
                    case WaitTypes.ElementNotExist:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_ElementNotExists(waitHelper.WebElement));
                        break;
                    case WaitTypes.TextNotPresent:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_TextNotPresent(waitHelper.WebElement, waitHelper.TextToWait));
                        break;
                    case WaitTypes.TextPresent:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_TextPresent(waitHelper.WebElement, waitHelper.TextToWait));
                        break;
                    case WaitTypes.ElementIsVisibleAndEnabled:
                        webDriverWait.Until(BeazleyWebDriverExtensions.ExpectedConditions_IsVisibleAndEnabled(waitHelper.WebElement));
                        break;
                }
            }
            else
            {

                var waitHelper = (WaitTitleHelper)this;
                var webDriverWait = new WebDriverWait(waitHelper.Driver, Timeout ?? BasePage.DefaultTimeout);
                if (!string.IsNullOrWhiteSpace(timeoutMessage))
                    webDriverWait.Message = timeoutMessage;
                webDriverWait.PollingInterval = PollingFrequency ?? BasePage.DefaultPollingFrequency;

                switch (WaitType)
                {
                    case WaitTypes.TitleIs:
                        Log.Info(string.Format("Waiting until _title becomes {0}... current Title = {1}", waitHelper.Title ?? "(null)", waitHelper.Driver.Title));
                        webDriverWait.Until(ExpectedConditions.TitleIs(waitHelper.Title));
                        Log.Info(string.Format("Wait ended for _title to become {0}... current Title = {1}", waitHelper.Title ?? "(null)", waitHelper.Driver.Title));
                        break;
                    case WaitTypes.TitleContains:
                        Log.Info(string.Format("Waiting until _title contains {0}... current Title = {1}", waitHelper.Title ?? "(null)", waitHelper.Driver.Title));
                        webDriverWait.Until(ExpectedConditions.TitleContains(waitHelper.Title));
                        Log.Info(string.Format("Wait ended for _title to contain {0}... current Title = {1}", waitHelper.Title ?? "(null)", waitHelper.Driver.Title));
                        break;
                }
            }
        }
    }


    public class Step2WaitHelper : Step3WaitHelper
    {
        public Step2WaitHelper(bool isElement)
            : base(isElement)
        { }

        public Step3WaitHelper WithPollingFrequency(TimeSpan duration)
        {
            Timeout = duration;
            return this;
        }
    }


    public class Step1WaitHelper : Step2WaitHelper
    {
        public Step1WaitHelper(bool isElement)
            : base(isElement)
        { }
        public Step2WaitHelper ForNoMoreThan(TimeSpan duration)
        {
            Timeout = duration;

            return this;
        }
    }


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WaitElementHelper : Step1WaitHelper
    {
        public WaitElementHelper(WebElement element)
            : base(true)
        {
            WebElement = element;
        }

        //public WaitElementHelper(By elementLocator)
        //    : base(true)
        //{
        //    WebElement = new WebElement();
        //}

        public WebElement WebElement { get; set; }

        public Step1WaitHelper BecomesVisible()
        {

            WaitType = WaitTypes.ElementIsVisible;
            return this;
        }

        public Step1WaitHelper BecomesNotVisible()
        {
            WaitType = WaitTypes.ElementIsNotVisible;
            return this;
        }

        public Step1WaitHelper Exists()
        {
            WaitType = WaitTypes.ElementExists;
            return this;
        }

        public Step1WaitHelper DoesNotExist()
        {
            WaitType = WaitTypes.ElementNotExist;
            return this;
        }

        public Step1WaitHelper BecomesEnabled()
        {
            WaitType = WaitTypes.ElementIsEnabled;
            return this;
        }

        public Step1WaitHelper BecomesDisabled()
        {
            WaitType = WaitTypes.ElementIsNotEnabled;
            return this;
        }

        public Step1WaitHelper IsNotStale()
        {
            WaitType = WaitTypes.ElementIsNotStale;
            return this;
        }

        public Step1WaitHelper DoesNotContain(string text)
        {
            WaitType = WaitTypes.TextNotPresent;
            TextToWait = text;
            return this;
        }

        public Step1WaitHelper DoesContain(string text)
        {
            WaitType = WaitTypes.TextPresent;
            TextToWait = text;
            return this;
        }

        public Step1WaitHelper BecomesVisibleAndEnabled()
        {
            WaitType = WaitTypes.ElementIsVisibleAndEnabled;
            return this;
        }
    }


    public class WaitTitleHelper : Step1WaitHelper
    {
        private IWebDriver driver;

        public WaitTitleHelper(IWebDriver driver)
            : base(false)
        {
            this.driver = driver;
        }

        private string _title;
        public string Title
        {
            set
            {
                _title = value;
            }
            get { return _title; }
        }

        public IWebDriver Driver { get { return driver; } }

        public Step1WaitHelper Is(string title)
        {
            WaitType = WaitTypes.TitleIs;
            _title = title;
            return this;
        }

        public Step1WaitHelper Contains(string titlePart)
        {
            WaitType = WaitTypes.TitleContains;
            _title = titlePart;
            return this;
        }
    }
}
