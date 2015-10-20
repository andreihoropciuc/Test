using System;
using Beazley.AutomationFramework.InfrastructureObjects;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.Selenium
{
    public static class WebElementExtensions
    {
        public static bool IsDisabled(this WebElement control)
        {
            return !control.Element.Enabled;
        }

        public static bool IsReadOnly(this WebElement control)
        {
            return (control.Element.GetAttribute("readOnly") != null && control.Element.GetAttribute("readOnly").Equals("true"));
        }

        public static bool HasClass(this WebElement control, string className)
        {
            var attribute = control.Element.GetAttribute("class");
            return (attribute != null && attribute.Contains(className));
        }

        public static bool IsTextbox(this WebElement control)
        {
            var attribute = control.Element.GetAttribute("type");
            return (attribute != null && attribute.Equals("text"));
        }

        public static void TryAction(this WebElement control, Action action, Predicate<IAlert> success, int retries = 3)
        {
            while (!success(null) && retries > 0)
            {
                action();
                retries--;
            }
        }
    }
}
