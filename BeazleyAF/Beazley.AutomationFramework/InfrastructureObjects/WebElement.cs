using System;
using System.Data;
using Beazley.AutomationFramework.Enums;
using Beazley.AutomationFramework.Selenium;
using log4net;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    public class WebElement
    {
        public readonly string Identifier;
        public BasePage ParentPage { get; private set; }
        protected ContainerWebElement ParentContainer;
        private Type NextPage { get; set; }        
        private ContainerType? NextPageContainerType { get; set; }         

        public WebElement(string identifier, BasePage parentPage, Type nextPage = null, ContainerType? containerType = null, ContainerWebElement parentContainer = null)
        {
            Identifier = identifier;
            ParentPage = parentPage;
            NextPage = nextPage;
            ParentContainer = parentContainer;
            NextPageContainerType = containerType;
        }

        protected WebElement(string testValue)
        {
        }

        private IWebElement _element;
        public IWebElement Element
        {
            get
            {
                if (_element == null)
                {             
                    return FindChildElement();
                }
                return _element;
            }
            set { _element = value; }
        }

        private readonly ILog _log = LogManager.GetLogger(typeof(BeazleyWebDriverTestBase));

        public bool Exists
        {
            get { return Element != null; }
        }

        public bool Displayed
        {
            get
            {
                return Element.Displayed;
            }
        }        

        public bool Enabled
        {
            get { return Element.Enabled; }
        }

        public bool Selected
        {
            get { return Element.Selected; }
        }

        public string Value
        {
            get
            {
                Until.Element(this).BecomesVisible().Wait();
                return Element.GetAttribute("value");
            }
        }

        public string InnerText
        {
            get
            {
                var element = Element;
                return element.Text;
            }
        }

        public bool ReadOnly()
        {
            if (!String.IsNullOrEmpty(Element.GetAttribute("disabled")) || !String.IsNullOrEmpty(Element.GetAttribute("readOnly")))
            {
                return true;
            }
            return false;
        }

        public static WebElement CreateModel(string value)
        {
            return new WebElement(value);
        }

        private IWebElement FindChildElement()
        {
            return ParentPage.Find(Identifier);
        }

        public void SendKeys(string text, bool waitForItself = true)
        {
            if (waitForItself)
                WaitForElementVisibleAndEnabled();

            Element.SendKeys(text);
        }

        public void SendKeysWithClickAndBlur(string text, bool waitForItself = true)
        {
            if (waitForItself)
                WaitForElementVisibleAndEnabled();

            Element.Click();
            Element.SendKeys(text);
            Blur();
        }

        /// <summary>
        /// Waits for current element to be Visible and Enabled
        /// </summary>
        public void WaitForElementVisibleAndEnabled()
        {
            _log.Info(String.Format("Waiting for element to become enabled. Page ({0}) = {1}", ParentPage.Title, ParentPage.Url));
            Until.Element(this).BecomesVisibleAndEnabled().Wait();		//wait for current control to be visible & enabled before editing it
            _log.Info(String.Format("Element has become enabled. Page ({0}) = {1}", ParentPage.Title, ParentPage.Url));
        }        

        /// <summary>
        /// Clicks and waits for the page to fully load: _title to change (if navigating to new page), updater overlay to dissapear, pending ajax calls to finish.
        /// VERY IMPORTANT: In case alerts appear use ClickAndTreatAlert() as it detects alerts
        /// </summary>
        /// <param name="waitForItself"></param>
        /// <param name="waitForPageToLoad"></param>
        /// <param name="alertResolveStatus">The way the popup/alert is treated. True = accept alert, False = cancel alert, DontResolve = leave alert</param>
        /// <param name="timeToWaitForAlert">Represents the time allocated for detecting the alert popup. Applies only to Accept and Dismiss alert resolve statuses. Value is in milliseconds</param>
        /// <param name="skipPageCreation"></param>
        /// <param name="waitForPageToLoadAfterAlert"></param>
        /// <returns></returns>
        public virtual object Click(bool waitForItself = true, bool waitForPageToLoad = true, AlertResolveType alertResolveStatus = AlertResolveType.TreatAfter, int timeToWaitForAlert = 500, bool skipPageCreation = false, bool waitForPageToLoadAfterAlert = true)
        {
            ParentPage.WaitForDynamicContentToLoad();

            var result = ClickNoWait(waitForItself, skipPageCreation);
            var newPage = (result != null);
            bool pageAlert;

            if (newPage)
            {
                ((BasePage)result).WaitForAlert(timeToWaitForAlert);	//check for alert popups as default, but for treating them use ClickAndTreatAlert()
                _log.Info(String.Format("Click(): Page alert popup status = {0}.", ((BasePage)result).IsAlertPresent));
                pageAlert = ((BasePage)result).IsAlertPresent;
            }
            else
            {
                ParentPage.WaitForAlert(timeToWaitForAlert);
                _log.Info(String.Format("Click(): Page alert popup status = {0}", ParentPage.IsAlertPresent));
                pageAlert = ParentPage.IsAlertPresent;
            }

            if (pageAlert)
            {
                _log.Info(String.Format("Alert Message: {0}", ParentPage.Alert.Text));
                switch (alertResolveStatus)
                {
                    case AlertResolveType.Accept:
                        if (newPage)
                            ((BasePage)result).AcceptAlert(waitForPageToLoadAfterAlert);
                        else
                            ParentPage.AcceptAlert(waitForPageToLoadAfterAlert);
                        break;
                    case AlertResolveType.Dismiss:
                        if (newPage)
                            ((BasePage)result).DismissAlert(waitForPageToLoadAfterAlert);
                        else
                            ParentPage.DismissAlert(waitForPageToLoadAfterAlert);
                        break;
                    case AlertResolveType.TreatAfter:
                        break;
                }
            }

            if (waitForPageToLoad && !pageAlert)
            {
                if (!newPage)
                {
                    ParentPage.WaitForDynamicContentToLoad();
                }
            }

            return result;
        }

        public object GetPageInstance(ContainerWebElement popup)
        {
            var page = Activator.CreateInstance(NextPage, ParentPage.Driver, NextPageContainerType, null, popup) as BasePage;

            return page;
        }

        /// <summary>
        /// Emulates click on current IWebElement without waiting for the page to load
        /// </summary>
        /// <returns> return value should be ignored </returns>
        private object ClickNoWait(bool waitForItself = true, bool skipPageCreation = false)
        {
            _log.Info(String.Format("Start click: on element {0} from page {1}", Identifier, ParentPage.GetType().FullName));

            if (waitForItself)
            {
                _log.Info(String.Format("performStaleTest = true -> Performing StaleTest before Clicking button {0}", Identifier));
                Until.Element(this).IsNotStale().Wait();
            }

            if (!Element.Displayed)
            {
                _log.Info(String.Format("button.Displayed = false -> Waiting for button to Become Visible before Clicking {0}", Identifier));
                Until.Element(this).BecomesVisible().Wait();
            }

            if (!Element.Enabled)
            {
                _log.Info(String.Format("button.Enabled = false -> Waiting for button to Become Enabled before Clicking {0}", Identifier));
                Until.Element(this).BecomesEnabled().Wait();
            }

            if (!Element.Displayed || !Element.Enabled)
                throw new ConstraintException("Button is not displayed or not enabled !");
            //Until.Element(this.Element).BecomesVisible().Wait();

            _log.Info(String.Format("Clicking -> [{0}]", Identifier));

            if (NextPage == null || skipPageCreation)
            {
                //Actions builder = new Actions(Driver);
                //builder.MoveToElement(this.Element).Click(this.Element).Perform();	//this is to fix: "The point at which the driver is attempting to click on the element was not scrolled into the viewport."
                Element.Click();
            }
            else
            {
                if (Globals.RegisterTrigger(NextPage, this))
                {
                    Globals.TriggerNavigationFor(NextPage);
                }

                _log.Info(String.Format("Before Page {0} instantiation", NextPage.Name));

                BasePage page;

                try
                {
                    page = Activator.CreateInstance(NextPage, ParentPage.Driver, NextPageContainerType, null, ParentContainer) as BasePage;

                }
                catch (UnhandledAlertException e)
                {
                    _log.Info(String.Format("Unhandled Alert Excleption with Text = {0} when pressing button = {1} returning null !", e.AlertText, Identifier));
                    throw;
                }
                finally
                {
                    Globals.UnregisterTrigger(NextPage);
                }

                _log.Info(String.Format("After Page {0} instantiation", NextPage.Name));

                _log.Info(String.Format("Finish click: on element {0} from page {1}", Identifier, ParentPage.GetType().FullName));

                return page;
            }
            _log.Info(String.Format("Finish click: on element {0} from page {1}", Identifier, ParentPage.GetType().FullName));

            return null;
        }

        /// <summary>
        /// Clicks and handles alert by accepting it, dismissing it or just waits for it and doesn't treat it automatically.
        /// </summary>
        /// <param name="alertResolveStatus">Accepts, dissmisses or doesn't treats the alert.</param>
        /// <param name="timeToWaitForAlert">Represents the time allocated for detecting the alert popup. Applies only to Accept and Dismiss alert resolve statuses. Value is in milliseconds.</param>
        /// <param name="skipPageCreation"></param>
        /// <param name="waitForPageToLoadAfterAlert"></param>
        /// <returns></returns>
        public virtual object ClickAndTreatAlert(AlertResolveType alertResolveStatus = AlertResolveType.Accept, int timeToWaitForAlert = 60000, bool skipPageCreation = false, bool waitForPageToLoadAfterAlert = true)
        {
            if (alertResolveStatus == AlertResolveType.TreatAfter)
                return Click(true, false, alertResolveStatus, timeToWaitForAlert, false, false);
            return Click(true, true, alertResolveStatus, timeToWaitForAlert, skipPageCreation, waitForPageToLoadAfterAlert);
        }

        /// <summary>
        /// Also waits for dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish) - due to some controls triggering overlay loader or ajax calls after being edited
        /// </summary>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void Blur(bool waitForItself = true, bool waitForDynamicContentLoad = true)	//param waitForItself must be default true because stale test needs to happen when it's used directly on control
        {
            if (waitForItself)
                WaitForElementVisibleAndEnabled();

            Element.SendKeys("\t");		//Keys.Tab

            if (waitForDynamicContentLoad)
                ParentPage.WaitForDynamicContentToLoad();
        }

        /// <summary>
        /// Also waits for dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish) - due to some controls triggering overlay loader or ajax calls after being edited
        /// </summary>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void Clear(bool waitForItself = true, bool waitForDynamicContentLoad = true) //param waitForItself must be default true because stale test needs to happen when it's used directly on control
        {
            if (waitForItself)
                WaitForElementVisibleAndEnabled();

            //			this.Element.Click();
            Element.Clear();

            if (waitForDynamicContentLoad)
                ParentPage.WaitForDynamicContentToLoad();
        }

        public void ClearWithClick(bool waitForItself = true, bool waitForDynamicContentLoad = true) //param waitForItself must be default true because stale test needs to happen when it's used directly on control
        {
            if (waitForItself)
                WaitForElementVisibleAndEnabled();

            Element.Click();
            Element.Clear();

            if (waitForDynamicContentLoad)
                ParentPage.WaitForDynamicContentToLoad();

            Blur(false);

            if (waitForDynamicContentLoad)
                ParentPage.WaitForDynamicContentToLoad();
        }
    }
}
