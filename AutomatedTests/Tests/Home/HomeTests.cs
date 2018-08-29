using AutomatedTests.Browser.Models;
using AutomatedTests.Pages;
using NUnit.Framework;

namespace AutomatedTests.Tests.Home
{
    [TestFixture, Category("Home")]
    public class HomeTests
    {
        [Test]
        public void GoToHomepage()
        {
            using (var setup = new Setup(Drivers.Chrome))
            {
                setup.Browser.Visit(setup.Config["Url"]);

                Assert.IsTrue(setup.Browser.CurrentPage<HomePage>().Home().VerifyPageInformation());
            }
        }
    }
}
