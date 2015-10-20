using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using Beazley.AutomationFramework.InfrastructureObjects;
using log4net;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework
{
    public class Globals
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Globals));

        private static TimeSpan _implicitWaitDefault;
        private static TimeSpan _implicitWait;
        private static IWebDriver _driver;
        private static ITestConfiguration _testConfiguration;

        private static readonly Stack<TimeSpan> TimeoutStack = new Stack<TimeSpan>();

        internal static void Setup(IWebDriver driver, TimeSpan waitDefault)
        {
            _implicitWait = _implicitWaitDefault = waitDefault;
            _driver = driver;
        }

        internal static void Setup(ITestConfiguration testConfiguration)
        {
            _testConfiguration = testConfiguration;
        }

        public static TimeSpan ImplicitWaitDefault
        {
            get { return _implicitWaitDefault; }
        }

        public static TimeSpan ImplicitWait
        {
            get { return _implicitWait; }
        }

        public static IWebDriver Driver
        {
            get
            {
                if (_driver == null)
                    throw new InstanceNotFoundException("Driver was not initialized properly !");

                return _driver;
            }
        }

        public static ITestConfiguration Configuration
        {
            get
            {
                if (_testConfiguration == null)
                    throw new InstanceNotFoundException("Test Configuration was not initialized properly !");

                return _testConfiguration;
            }
        }

        internal static ITimeouts ChangeImplicitWait(TimeSpan timeToWait)
        {
            TimeoutStack.Push(_implicitWait);
            //Log.Info(string.Format("Globals::ChangeImplicitWait -> Pushing = {0}", implicitWait));
            _implicitWait = timeToWait;
            var result = Driver.Manage().Timeouts().ImplicitlyWait(_implicitWait);
            //Log.Info(string.Format("Globals::ChangeImplicitWait -> ImplicitTimeout is now = {0}.", timeToWait));
            return result;
        }

        internal static ITimeouts RestoreImplicitWait()
        {
            var timeToWait = TimeoutStack.Count > 0 ? TimeoutStack.Pop() : ImplicitWaitDefault;
            //Log.Info(string.Format("Globals::RestoreImplicitWait -> Popping = {0}", timeToWait));
            _implicitWait = timeToWait;
            var result = Driver.Manage().Timeouts().ImplicitlyWait(timeToWait);
            //Log.Info(string.Format("Globals::RestoreImplicitWait -> ImplicitTimeout is now = {0}", timeToWait));
            return result;
        }

        private static readonly Dictionary<Type, WebElement> NavigationTriggers = new Dictionary<Type, WebElement>();

        public static bool RegisterTrigger(Type page, WebElement webElement)
        {
            lock (NavigationTriggers)
            {
                var newlyAdded = !NavigationTriggers.ContainsKey(page);

                NavigationTriggers[page] = webElement;

                return newlyAdded;
            }
        }

        public static void UnregisterTrigger(Type page)
        {
            lock (NavigationTriggers)
            {
                if (NavigationTriggers.ContainsKey(page))
                    NavigationTriggers.Remove(page);
            }
        }

        public static void TriggerNavigationFor(Type page)
        {
            lock (NavigationTriggers)
            {
                if (NavigationTriggers.ContainsKey(page))
                {
                    var webElement = NavigationTriggers[page];

                    Log.Info(string.Format("Clicking -> [{0}]", webElement.Identifier));

                    try
                    {
                        webElement.Element.Click();
                    }
                    catch (StaleElementReferenceException sere)
                    {
                        Log.Info("StaleElementReferenceException on TriggerNavigationFor(): " + sere);
                    }

                    Log.Info(string.Format(">>> Triggered Navigation for {0} : Clicked {1}!", page, webElement.Identifier));
                }
                else
                {
                    Log.Info("Navigation Trigger not present!");
                }
            }
        }
    }
}
