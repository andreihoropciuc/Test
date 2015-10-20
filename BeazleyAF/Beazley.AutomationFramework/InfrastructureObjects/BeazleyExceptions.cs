using System;

namespace Beazley.AutomationFramework.InfrastructureObjects
{
    class BeazleyException : Exception
    {
        public BeazleyException(string message)
            : base(message)
        { }
    }

    class ElementNotPresentException : BeazleyException
    {
        public ElementNotPresentException(WebElement webElement)
            : base(string.Format("Element {0} no longer exists on page", webElement))
        {

        }
    }

    public class TestSetupException : ApplicationException
    {
        public TestSetupException()
        {
        }

        public TestSetupException(string message)
            : base(message)
        {
        }

        public TestSetupException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
