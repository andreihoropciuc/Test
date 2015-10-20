using Beazley.AutomationFramework.InfrastructureObjects;
using OpenQA.Selenium;

namespace Beazley.Pages
{
    public class MyBeazleyHomePage : BasePage
    {
        public WebElement SignOutButton;

        public MyBeazleyHomePage(IWebDriver driver, string title, string url, ContainerWebElement parentContainer = null)
            : base(driver, "myBeazley UAT - Quotes", url, parentContainer)
        {
            SignOutButton = new LinkWebElement<MyBeazleyLoginPage>("logoff", this);
        }

        public MyBeazleyLoginPage SignOut()
        {
            return SignOutButton.Click(false) as MyBeazleyLoginPage;
        }        
    }
}
