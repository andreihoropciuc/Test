using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Beazley.AutomationFramework.Enums;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    public class DropdownWebElement : WebElement
    {
        #region [Properties]

        private SelectElement Select
        {
            get
            {
                return new SelectElement(Element);
            }
        }

        private enum SelectionKind
        {
            Text,
            Value,
            Index
        }

        private Dictionary<SelectionKind, Action<object>> Selector { get; set; }

        public String SelectedOptionText
        {
            get
            {
                //Until.Element(this).IsNotStale().Wait();
                //Assert.IsTrue(Element.Displayed && Element.Enabled);
                return Select.SelectedOption.Text;
            }
        }

        public String SelectedOptionValue
        {
            get
            {
                return Select.SelectedOption.GetAttribute("value");
            }
        }

        public int ItemCount
        {
            get
            {
                return Element.FindElements(By.XPath("./descendant::option")).Count;
            }
        }

        public ReadOnlyCollection<IWebElement> GetItems
        {
            get
            {
                return Element.FindElements(By.XPath("./descendant::option"));
            }
        }

        #endregion [Properties]

        #region [Constructors]

        public DropdownWebElement(string identifier, BasePage parentPage, ContainerType? containerType = null, ContainerWebElement container = null)
            : base(identifier, parentPage, null, containerType, container)
        {
            Selector = new Dictionary<SelectionKind, Action<object>>
            {
					{SelectionKind.Text, x => Select.SelectByText((string) x)},
					{SelectionKind.Value, x => Select.SelectByValue((string) x)},
					{SelectionKind.Index, x => Select.SelectByIndex((int) x)}
				};
        }

        public DropdownWebElement(string value)
            : base(value)
        {
        }

        #endregion [Constructors]

        #region [Methods]

        public new static DropdownWebElement CreateModel(string value)
        {
            return new DropdownWebElement(value);
        }

        private void LogSelection(SelectionKind selectionKind, object selection, bool blur)
        {
            Trace.WriteLine(string.Format("Selected option {0} by {1} from {2} ", selectionKind, selection, Identifier));
        }

        public int GetOptionGroupItemCount(string optionGroupName)
        {
            return Element.FindElements(By.XPath("./descendant::optgroup[@label='" + optionGroupName + "']/descendant::option")).Count;
        }

        public string GetOptionTextByIndex(int p)
        {
            return Element.FindElement(By.XPath("./descendant::option[" + p + "]")).Text;
        }

        #endregion [Methods]




        #region [Action Methods]

        public void PerformActionAndWaitForPageToLoad(Action<DropdownWebElement> action, DropdownWebElement webElement = null)
        {
            action(webElement ?? this);
            if (ParentPage is ContainerBasePage)
                (ParentPage as ContainerBasePage).WaitForPageToLoad();
            else
                ParentPage.WaitForPageToLoad();
        }

        private void SelectBy(SelectionKind selectionKind, object selection, bool blur, bool waitForItself = false, bool waitForDynamicContentLoad = true)
        {
            if (waitForItself)
                WaitForElementVisibleAndEnabled();

            //Assert.IsTrue(Element.Displayed, string.Format("Element {0} is not Displayed", Locator));		//TODO
            //Assert.IsTrue(Element.Enabled, string.Format("Element {0} is not Enabled", Locator));			//TODO

            Selector[selectionKind](selection);

            if (waitForDynamicContentLoad)
                ParentPage.WaitForDynamicContentToLoad();

            if (blur)
                Blur(true, false);

            if (waitForDynamicContentLoad)
                ParentPage.WaitForDynamicContentToLoad();

            LogSelection(selectionKind, selection, blur);
        }

        /// <summary>
        /// Also waits for page dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="blur"></param>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void SelectByText(string text, bool blur = false, bool waitForItself = true, bool waitForDynamicContentLoad = true)
        {
            SelectBy(SelectionKind.Text, text, blur, waitForItself, waitForDynamicContentLoad);
        }

        [Obsolete]
        public void SelectByTextAndWait(string text)
        {
            SelectBy(SelectionKind.Text, text, false, true, true);
        }


        /// <summary>
        /// Also waits for page dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="blur"></param>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void SelectByTextAndBlur(string text, bool blur = true, bool waitForItself = true, bool waitForDynamicContentLoad = true)
        {
            SelectBy(SelectionKind.Text, text, blur, waitForItself, waitForDynamicContentLoad);
        }

        /// <summary>
        /// Also waits for page dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="blur"></param>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void SelectByValue(string value, bool blur = false, bool waitForItself = true, bool waitForDynamicContentLoad = true)
        {
            SelectBy(SelectionKind.Value, value, blur, waitForItself, waitForDynamicContentLoad);
        }

        /// <summary>
        /// Also waits for page dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="blur"></param>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void SelectByValueAndBlur(string value, bool blur = true, bool waitForItself = true, bool waitForDynamicContentLoad = true)
        {
            SelectBy(SelectionKind.Value, value, blur, waitForItself, waitForDynamicContentLoad);
        }

        /// <summary>
        /// Also waits for page dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blur"></param>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void SelectByIndex(int index, bool blur = false, bool waitForItself = true, bool waitForDynamicContentLoad = true)
        {
            SelectBy(SelectionKind.Index, index, blur, waitForItself, waitForDynamicContentLoad);
        }

        [Obsolete]
        public void SelectByIndexAndWait(int index)
        {
            SelectBy(SelectionKind.Index, index, true, true, true);
        }


        /// <summary>
        /// Also waits for page dynamic contents to load (updater overlay to dissapear, pending ajax calls to finish)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blur"></param>
        /// <param name="waitForItself"></param>
        /// <param name="waitForDynamicContentLoad"></param>
        public void SelectByIndexAndBlur(int index, bool blur = true, bool waitForItself = true, bool waitForDynamicContentLoad = true)
        {
            SelectBy(SelectionKind.Index, index, blur, waitForItself, waitForDynamicContentLoad);
        }

        #endregion [Action Methods]
    }
}
