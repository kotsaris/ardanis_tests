using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AutomatedTests.Browser
{
    public abstract class PageBuilder
    {
        protected readonly IWebDriver WebDriver;

        protected PageBuilder(IWebDriver driver)
        {
            WebDriver = driver;
        }

        // Sets the file to upload to the browser
        protected void UploadFile(IWebElement uploadButton, string xpath, string fullFilePath)
        {
            ScrollWindowToElement(uploadButton);

            WebDriver.FindElement(By.XPath(xpath)).SendKeys(fullFilePath);

            // Use for PhantomJS
            //((PhantomJSDriver) WebDriver).ExecutePhantomJS("var page = this; page.uploadFile('input[type=file]', 'C:/Git/campaign-builder/server/Scenarios/Assets/Images/1200x640/1200x640.jpg');");
        }

        // Scrolls the view port of the browser window to the top of the page
        protected void ScrollWindowToTopOfThePage()
        {
            Thread.Sleep(2000);
            try
            {
                ((IJavaScriptExecutor)WebDriver).ExecuteScript("scroll(0, 0)");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[WARNING] ScrollWindowToTopOfThePage() has failed with the exception {e}");
            }
        }

        // Scrolls the view port down to the specified element
        protected void ScrollWindowToElement(IWebElement element)
        {
            var nativeElement = element;
            var x = nativeElement.Location.X;
            var y = nativeElement.Location.Y;
            var scroller = string.Format("scroll({0}, {1})", x, y);
            ((IJavaScriptExecutor)WebDriver).ExecuteScript(scroller);
        }

        protected void CloseAlerts()
        {
            WaitForSeconds(5);
            try
            {
                var alert = WebDriver.SwitchTo().Alert();

                if (alert != null)
                {
                    Console.WriteLine("[INFO] Closing alert window with text: {0}", alert.Text);
                    alert.Accept();
                }
            }
            catch (NoAlertPresentException) { }
            catch (WebDriverException) { }
        }

        protected void RefreshBrowser()
        {
            WebDriver.Navigate().Refresh();
        }

        protected string GetUrl()
        {
            return WebDriver.Url;
        }

        protected static void WaitForSeconds(int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        protected bool RefreshPageAndWaitForElement(From from, IWebElement elementToWait, int timesToWait = 6, int secondsToWaitEachTime = 2)
        {
            for (var i = 0; i <= timesToWait; i++)
            {
                RefreshBrowser();
                WaitForSeconds(secondsToWaitEachTime);

                if (elementToWait != null)
                {
                    return true;
                }
            }

            return false;
        }

        protected void SelectOption(IWebElement element, string option, int attempts = 5)
        {
            for (int i = 0; i <= attempts; i++)
            {
                try
                {
                    new SelectElement(element).SelectByText(option);
                }
                catch (Exception)
                {
                    if (i == attempts)
                    {
                        throw;
                    }
                }
            }
        }

        protected string GetSelectedComboboxElement(IWebElement element)
        {
            var selectedValue = new SelectElement(element);
            return selectedValue.SelectedOption.Text;
        }

        protected void HoverOver(IWebElement element)
        {
            Actions action = new Actions(WebDriver);
            action.MoveToElement(element).Perform();
        }

        /******************************/
        /** Page Object Constructors **/
        /******************************/

        protected enum From
        {
            Xpath,
            Css,
        }

        protected IWebElement Element<T>(From from, T pageObject, bool failOnError = true, int maxAttempts = 15, int secondsToWait = 5) where T: ObjectBuilder
        {
            var selector = GetSelector(from, pageObject.Path);

            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(secondsToWait));

            for (var i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(selector));
                }
                catch (Exception)
                {
                    if (i == maxAttempts)
                    {
                        if (failOnError)
                        {
                            PrintElementNotFoundMessage(pageObject);
                            throw;
                        }
                        return null;
                    }
                }
            }

            Thread.Sleep(100);
            return WebDriver.FindElement(selector);
        }

        protected IEnumerable<IWebElement> Elements<T>(From from, T pageObject, bool failOnError = true, int maxAttempts = 10, int secondsToWait = 5) where T: ObjectBuilder
        {
            var selector = GetSelector(from, pageObject.Path);

            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(secondsToWait));

            for (var i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.PresenceOfAllElementsLocatedBy(selector));
                    break;
                }
                catch (Exception)
                {
                    if (i == maxAttempts)
                    {
                        if (failOnError)
                        {
                            PrintElementNotFoundMessage(pageObject);
                            throw;
                        }
                        return null;
                    }
                }
            }

            return WebDriver.FindElements(selector);
        }

        private By GetSelector(From from, string elementPath)
        {
            switch (from)
            {
                case From.Xpath:
                    return By.XPath(elementPath);
                case From.Css:
                    return By.CssSelector(elementPath);
                default:
                    throw new NotImplementedException();
            }
        }

        private void PrintElementNotFoundMessage<T>(T pageObject) where T: ObjectBuilder
        {
            Console.WriteLine($"[FAILED] Failed to find the element {pageObject.Name} "+
                $"at the page {pageObject.Page} with the locator {pageObject.Path}");
        }
    }
}
