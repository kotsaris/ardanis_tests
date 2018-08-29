using AutomatedTests.Browser;
using OpenQA.Selenium;

namespace AutomatedTests.Pages.Home
{
    public class HomeFunctions : PageBuilder
    {
        public HomeFunctions(IWebDriver driver) : base(driver){}

        public bool VerifyPageInformation()
        {
            if (!Element(From.Xpath, HomeObjects.HomeLink).Displayed
                || !Element(From.Xpath, HomeObjects.WhatWeDoLink).Displayed
                || !Element(From.Xpath, HomeObjects.HowWeDoItLink).Displayed
                || !Element(From.Xpath, HomeObjects.OurServicesLink).Displayed
                || !Element(From.Xpath, HomeObjects.WhyChooseUsLink).Displayed
                || !Element(From.Xpath, HomeObjects.WhoWeAreLink).Displayed
                || !Element(From.Xpath, HomeObjects.BlogLink).Displayed
                || !Element(From.Xpath, HomeObjects.ContactUsLink).Displayed)
            {
                return false;
            }

            return true;
        }
    }
}
