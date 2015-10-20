using System;
using Beazley.AutomationFramework.InfrastructureObjects;
using Beazley.AutomationFramework.Selenium;
using Beazley.Pages;
using NUnit.Framework;

namespace Beazley.Tests.SmokeTests
{
    [TestFixture]
    public class BeazleyLoginPageTest : BeazleyWebDriverTestBase
    {
        string pageTitle = "myBeazley UAT";
        private MyBeazleyLoginPage _beazleyLoginPage;
        private MyBeazleyHomePage _beazleyHomePage;

        public override void BeforeAll()
        {
            _beazleyLoginPage = Website.GoTo<MyBeazleyLoginPage>();            
        }
        
        [Test]
        public void A01_UiElementsMyBeazleyLogin()
        {

            //Logo
            _beazleyLoginPage.LinkLogoBeazley.Click();
            Assert.IsTrue(_beazleyLoginPage.LinkLogoBeazley.Displayed);

            //Header logo
            Assert.IsTrue(_beazleyLoginPage.HeaderLogo.Displayed);

            //Beazley Right Image
            Assert.IsTrue(_beazleyLoginPage.BeazleyRightImage.Displayed);

            //Login page title
            _beazleyLoginPage.CheckLoginPageTitle(pageTitle);

            //PrivacyLinks
            Assert.IsTrue(_beazleyLoginPage.PrivacyLinks.Displayed);
            Assert.IsTrue(_beazleyLoginPage.TcPrivacyLinks.Displayed);
            Assert.IsTrue(_beazleyLoginPage.PcPrivacyLinks.Displayed);

            Assert.AreEqual("Terms and Conditions", _beazleyLoginPage.TcPrivacyLinks.InnerText);
            Assert.AreEqual("Privacy and cookies statements", _beazleyLoginPage.PcPrivacyLinks.InnerText);
        }

        [Test]
        public void A02_CheckUiLoginForm()
        {
            //Login title lable
            Assert.IsTrue(_beazleyLoginPage.TitleLabelLogin.Displayed);
            Assert.IsTrue(_beazleyLoginPage.TitleLabelLogin.InnerText.Equals("Sign in"));

            //Login sign in button
            Assert.IsTrue(_beazleyLoginPage.BtnSignIn.Displayed);
            Assert.IsTrue(_beazleyLoginPage.TitleLabelLogin.InnerText.Equals("Sign in"));

            //Login user name
            Assert.IsTrue(_beazleyLoginPage.UsernameField.Displayed);
            Assert.IsTrue(_beazleyLoginPage.UsernameLabel.Displayed);
            Assert.IsTrue(_beazleyLoginPage.UsernameLabel.InnerText.Equals("Username"));

            //Login password
            Assert.IsTrue(_beazleyLoginPage.PasswordField.Displayed);
            Assert.IsTrue(_beazleyLoginPage.PasswordLabel.Displayed, "Password label is missing!");
            Assert.AreEqual("Password", _beazleyLoginPage.PasswordLabel.InnerText);

            //Forgotten Password link
            Assert.IsTrue(_beazleyLoginPage.ForgottenPassword.Displayed);
            Assert.IsTrue(_beazleyLoginPage.ForgottenPassword.InnerText.Equals("Forgotten your password?"));

            //Language selector
            Assert.IsTrue(_beazleyLoginPage.LanguageLocator.Displayed);
            Assert.IsTrue(_beazleyLoginPage.LanguageLocator.Enabled);
            Console.WriteLine("#### this.LanguageSelector.SelectedOptionText: " + _beazleyLoginPage.LanguageLocator.InnerText);
            Assert.IsTrue(_beazleyLoginPage.LanguageLocator.InnerText.Equals("UK"));
        }

        [Test]
        public void A04_CheckInvalidLogin()
        {
            _beazleyLoginPage.CheckInvalidLogin("cip", "cip");

            Assert.IsTrue(_beazleyLoginPage.ErrorLoginMessage.Displayed);
            Console.WriteLine("###");
            Console.WriteLine("ERROR MESSAGE: " + _beazleyLoginPage.ErrorLoginMessage.InnerText);
            Console.WriteLine("###");
            //Assert.IsTrue(this.ErrorLoginMessage.InnerText.Equals("Validation Messages:\nThe user name or password provided is incorrect."));
            Assert.AreEqual("Validation Messages:\r\nThe user name or password provided is incorrect.", _beazleyLoginPage.ErrorLoginMessage.InnerText);
        }

        [Test]
        public void A03_CheckEmptyInputError()
        {
            _beazleyLoginPage.CheckEmptyInputError();

            Assert.IsTrue(_beazleyLoginPage.EmptyUsernameError.Displayed);
            Assert.AreEqual("* Invalid Username", _beazleyLoginPage.EmptyUsernameError.InnerText);
        }

        [Test]
        public void User_Can_Login()
        {            
            _beazleyHomePage = _beazleyLoginPage.SuccessfullLogin();

            Assert.IsTrue(_beazleyHomePage.SignOutButton.Displayed);
        }
    }
}
