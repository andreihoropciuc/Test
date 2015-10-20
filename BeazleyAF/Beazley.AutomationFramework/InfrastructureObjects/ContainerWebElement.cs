using Beazley.AutomationFramework.Enums;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    public class ContainerWebElement : WebElement
    {
        private bool _isActive;

        public ContainerWebElement(string identifier, BasePage parentPage, ContainerType containerType = ContainerType.Element, ContainerWebElement container = null)
            : base(identifier, parentPage, null, containerType, container)
        {
            Type = containerType;
            WindowName = null;
        }

        public ContainerType Type { get; set; }
        public string WindowName { get; set; }
        public bool IsActive { get { return _isActive; } }

        public void Switch()
        {
            IWebElement container = Element;

            if (HasContainerOrWindowName(container))
            {
                //containerSwitchers[Type](driver);
                _isActive = true;
            }
        }

        public void Restore()
        {
            ParentPage.Driver.SwitchTo().DefaultContent();
            _isActive = false;
        }

        #region Private Methods

        private bool HasContainerOrWindowName(IWebElement container)
        {
            return container != null || !string.IsNullOrWhiteSpace(WindowName);
        }

        #endregion
    }
}
