using System;
using System.Linq;
using Beazley.AutomationFramework.Contexts;
using Beazley.AutomationFramework.Enums;
using Beazley.AutomationFramework.Interfaces;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class ContainerBasePage : BasePage, IContainerPage, IDisposable
    {

        public ContainerBasePage(IWebDriver driver, ContainerType? containerType, string title = null, string url = null, ContainerWebElement parentContainer = null)
            : base(driver, title, url, parentContainer)
        {
            if (containerType != null) Type = containerType.Value;
            ActionsBeforeInit();
            InitContainerContext();
        }

        private void ActionsBeforeInit()
        {

        }

        public ContainerType Type { get; set; }

        public ContainerWebElement Container { get; set; }


        private bool ForceNoSwitch = false;


        private ContainerContext _containerContext;
        private void InitContainerContext()
        {
            if (_containerContext == null || !_containerContext.IsActive)
                _containerContext = new ContainerContext(this);
        }

        public BasePage ParentPage
        {
            get { return Container.ParentPage; }
        }

        public string NewestWindowHandle
        {
            get
            {
                return Driver.WindowHandles.FirstOrDefault(handle => !WindowHandlesSnapshot.Contains(handle));
            }
        }

        public void Dispose()
        {
            _containerContext.Dispose();
        }

        public ContainerContext GetContainerContext()
        {
            if (_containerContext == null || !_containerContext.IsActive)
                _containerContext = new ContainerContext(this);

            return _containerContext;
        }

        internal bool DoesntNeedSwitch()
        {
            var containingPage = ParentPage as ContainerBasePage;

            if (containingPage == null)
                return false;

            if (ForceNoSwitch)
            {
                return true;
            }

            return false;
        }
    }  
}
