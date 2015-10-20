using Beazley.AutomationFramework.InfrastructureObjects;
using Beazley.AutomationFramework.Selenium;
using Beazley.Pages;
using NUnit.Framework;

namespace Beazley.Tests.SmokeTests
{
    [TestFixture]
    public class BeazleyHomePageTest : BeazleyWebDriverTestBase
    {
        private MyBeazleyLoginPage _beazleyLoginPage;
        private MyBeazleyHomePage _beazleyHomePage;

        public override void BeforeAll()
        {
            _beazleyLoginPage = Website.GoTo<MyBeazleyLoginPage>();            
        }

        [Test]
        public void User_Can_SignOut()
        {
            _beazleyHomePage = _beazleyLoginPage.SuccessfullLogin();
            var loginPage = _beazleyHomePage.SignOut();

            Assert.IsTrue(loginPage.BtnSignIn.Displayed);
        }
    }
}
