using AutomatedTests.Browser;
using AutomatedTests.Pages.Home;
using OpenQA.Selenium;

namespace AutomatedTests.Pages
{
    public class HomePage : PageBuilder
    {
        public HomePage(IWebDriver driver) : base(driver) { }

        public HomeFunctions Home()
        {
            return new HomeFunctions(WebDriver);
        }
    }
}
