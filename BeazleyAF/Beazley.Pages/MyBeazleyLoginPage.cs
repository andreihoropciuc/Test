using Beazley.AutomationFramework.InfrastructureObjects;
using OpenQA.Selenium;

namespace Beazley.Pages
{
    public class MyBeazleyLoginPage : BasePage
    {
        public WebElement LinkLogoBeazley { get; set; }
        public WebElement PrivacyLinks { get; set; }
        public WebElement TcPrivacyLinks { get; set; }
        public WebElement PcPrivacyLinks { get; set; }
        public WebElement HeaderLogo { get; set; }
        public WebElement BeazleyRightImage { get; set; }
        public WebElement TitleLabelLogin { get; set; }
        public WebElement BtnSignIn { get; set; }
        public WebElement FailedBtnSignIn { get; set; }
        public WebElement UsernameLabel { get; set; }
        public WebElement UsernameField { get; set; }
        public WebElement PasswordLabel { get; set; }
        public WebElement PasswordField { get; set; }
        public WebElement ForgottenPassword { get; set; }
        public WebElement LanguageLocator { get; set; }
        public WebElement ErrorLoginMessage { get; set; }
        public WebElement EmptyUsernameError { get; set; }

        public MyBeazleyLoginPage(IWebDriver driver, string title = null, string url = null, ContainerWebElement parentContainer = null)
            : base(driver, "myBeazley UAT", url, parentContainer)
        {            
            //Beazley Link
            LinkLogoBeazley = new LinkWebElement<MyBeazleyLoginPage>("LinkLogoBeazley", this);

            //Privacy links - RB_PrivacyLinks
            PrivacyLinks = new LinkWebElement<MyBeazleyLoginPage>("PrivacyLinks", this);

            //Privacy links - RB_PrivacyLinks
            TcPrivacyLinks = new LinkWebElement<MyBeazleyLoginPage>("TC_PrivacyLinks", this);

            //Privacy links - RB_PrivacyLinks
            PcPrivacyLinks = new LinkWebElement<MyBeazleyLoginPage>("PC_PrivacyLinks", this);

            //Header logo
            HeaderLogo = new WebElement("HeaderLogo", this);

            //myBeazley right image
            BeazleyRightImage = new LinkWebElement<MyBeazleyLoginPage>("BeazleyRightImage", this);

            //Sign in title label
            TitleLabelLogin = new WebElement("TitleLabelLogin", this);

            //Sign in Submit
            BtnSignIn = new LinkWebElement<MyBeazleyHomePage>("BtnSignIn", this);

            //Sign in Submit
            FailedBtnSignIn = new WebElement("BtnSignIn", this);

            //Username field and label
            //UsernameField = new WebElement(Locator.Create(With.Id, "UsernameField"), this);
            UsernameField = new WebElement("UsernameField", this);
            UsernameLabel = new WebElement("UsernameLabel", this);

            //Password Field and label
            PasswordField = new WebElement("PasswordField", this);
            PasswordLabel = new WebElement("PasswordLabel", this);

            //Forgotten your password?
            ForgottenPassword = new LinkWebElement<MyBeazleyLoginPage>("ForgottenPassword", this);

            //DdlSelectLanguage = new SelectWebElement(Locator.Create(With.Id, "searchlang"), this);
            LanguageLocator = new WebElement("LanguageLocator", this);
            ErrorLoginMessage = new WebElement("ErrorLoginMessage", this);
            EmptyUsernameError = new WebElement("EmptyUsernameError", this); //loginerror
        }

        public void ClearLoginInputs()
        {
            UsernameField.Clear();
            PasswordField.Clear();
        }

        public bool CheckLoginPageTitle(string pageTitle)
        {
            return pageTitle == Title;
        }

        public MyBeazleyHomePage SuccessfullLogin()
        {
            ClearLoginInputs();
            UsernameField.SendKeys("Bluefin User 1");
            PasswordField.SendKeys("beazley02");            

            return BtnSignIn.Click() as MyBeazleyHomePage;
        }

        public void CheckInvalidLogin(string username, string password)
        {
            UsernameField.SendKeys(username);
            PasswordField.SendKeys(password);
            FailedBtnSignIn.Click(false);
            ErrorLoginMessage.WaitForElementVisibleAndEnabled();
        }

        public void CheckEmptyInputError()
        {
            LinkLogoBeazley.Click();
            FailedBtnSignIn.Click(false);
        }
    }
}
