using System;
using Beazley.AutomationFramework.Enums;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    public class LinkWebElement<T> : WebElement where T : BasePage
    {       
        public LinkWebElement(string identifier, BasePage parentPage, ContainerType? containerType = null, ContainerWebElement containerWebElement = null)
            : base(identifier, parentPage, typeof(T), containerType, containerWebElement)
        {
            ParentContainer = containerWebElement;
        }

		/// <summary>
		/// Clicks and waits for the page to fully load (title to change, updater overlay to dissapear, pending ajax calls to finish)
		/// </summary>
		/// <param name="waitForItself"></param>
		/// <param name="waitForPageToLoad"></param>
		/// <returns></returns>
	    public T Click(bool waitForItself = true, bool waitForPageToLoad = true)
		{
			ParentPage.LastKnownWindowHandle = ParentPage.Driver.CurrentWindowHandle;
			ParentPage.WindowHandlesSnapshot = ParentPage.Driver.WindowHandles;

			return base.Click(waitForItself, waitForPageToLoad) as T;
		}

		public T ClickAndTreatAlertAndOpenPopup(ContainerWebElement popupContainer, AlertResolveType alertResolveStatus = AlertResolveType.Accept, int timeToWaitForAlert = 60000)
		{
			ParentPage.LastKnownWindowHandle = ParentPage.Driver.CurrentWindowHandle;
			ParentPage.WindowHandlesSnapshot = ParentPage.Driver.WindowHandles;

			ClickAndTreatAlert(alertResolveStatus, timeToWaitForAlert, true);

			var page = GetPageInstance(popupContainer) as T;

			return page;
		}


        public T ClickAndTreatAlertAndOpenPage(AlertResolveType alertResolveStatus = AlertResolveType.Accept, int timeToWaitForAlert = 60000)
        {
            ParentPage.LastKnownWindowHandle = ParentPage.Driver.CurrentWindowHandle;
            ParentPage.WindowHandlesSnapshot = ParentPage.Driver.WindowHandles;
            
            ClickAndTreatAlert(alertResolveStatus, timeToWaitForAlert, true, false);

            var page = GetPageInstance(null) as T;

            return page;
        }

		public void PerformActionAndWaitForPageToLoad(Action<LinkWebElement<T>> action, LinkWebElement<T> webElement)
		{
			action(webElement??this);
			ParentPage.WaitForPageToLoad();
		}
    }
}
