using System;
using System.Collections.Generic;
using Beazley.AutomationFramework.Enums;
using Beazley.AutomationFramework.InfrastructureObjects;
using log4net;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.Contexts
{
    public class ContainerContext : IDisposable
    {
        protected readonly ILog Log = LogManager.GetLogger(typeof(ContainerContext));
        private readonly ContainerWebElement _containingElement;
        private readonly ContainerBasePage _containerPage;
        private readonly ContainerType _containerType;
        private readonly Dictionary<ContainerType, Action<IWebDriver>> _containerSwitchers;
        private readonly Dictionary<ContainerType, Action<IWebDriver>> _containerRestorers;
        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
        }
        public ContainerContext(ContainerBasePage container, ContainerContext parentContext = null)
        {
            _containingElement = container.Container; // the iframe
            _containerPage = container;
            _containerType = container.Type;
            _containerSwitchers = new Dictionary<ContainerType, Action<IWebDriver>>() 
            { 
                { ContainerType.Frame,   driver =>
                                             {
                                                 driver.SwitchTo().Frame(_containingElement.Element); Log.Info(string.Format("Switched to Frame {0}", _containingElement.Identifier));
                                             }},
                { ContainerType.Alert,   driver => driver.SwitchTo().Alert()},
                { ContainerType.Window,  driver => driver.SwitchTo().Window(_containerPage.NewestWindowHandle)},
                { ContainerType.Element, driver => driver.SwitchTo().ActiveElement() }
            };
            _containerRestorers = new Dictionary<ContainerType, Action<IWebDriver>>() 
            { 
                { ContainerType.Frame, driver =>
                    {
                        if (HasParentContext(parentContext))
                            SwitchToParentContext(parentContext);
                        else
                            SwitchToDefaultContent(driver);
                    }
                },
                { ContainerType.Alert,   driver => { driver.SwitchTo().DefaultContent(); Log.Info("Switched to DefaultContent"); }}, //?
                { ContainerType.Window,  driver => driver.SwitchTo().Window(_containerPage.LastKnownWindowHandle)},
                { ContainerType.Element, driver => driver.SwitchTo().ActiveElement() }  //?
            };
            Switch();
        }

        private bool HasParentContext(ContainerContext parentContext)
        {
            return parentContext != null;
        }

        private void SwitchToParentContext(ContainerContext parentContext)
        {
            parentContext.Switch();
            Log.Info("Switched to parentContext");
        }

        private void SwitchToDefaultContent(IWebDriver driver)
        {
            driver.SwitchTo().DefaultContent();
            Log.Info("Switched to DefaultContent");
        }

        public void Dispose()
        {
            Restore();
        }

        #region [Private Methods]
        private void Switch()
        {
            if (ContainerContextChangeNotNeeded())
                return;

            Log.Info(string.Format("Switching to {0}", _containerType));
            _containerSwitchers[_containerType](_containerPage.Driver);
            _isActive = true;
            //containerPage.WaitForPageToLoad();

        }

        private void Restore()
        {
            Log.Info("Entered ContainerContext.Restore()");
            if (ContainerContextChangeNotNeeded())
                return;

            Log.Info(string.Format("Restoring from {0}", _containerType));
            _containerRestorers[_containerType](_containerPage.Driver);
            _isActive = false;

        }

        private bool ContainerContextChangeNotNeeded()
        {
            return _containerType == ContainerType.Frame && _containerPage.DoesntNeedSwitch();
        }

        #endregion
    }
}
