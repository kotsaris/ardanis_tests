using AutomatedTests.Browser.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace AutomatedTests.Browser
{
    public class Browser
    {
        private readonly IWebDriver _webDriver;

        public Browser(Drivers drivers)
        {
            switch (drivers)
            {
                //case Drivers.Firefox:
                //    const string folderName = "temp";
                //    var profile = new FirefoxProfile { EnableNativeEvents = true };
                //    profile.SetPreference("browser.download.folderList", 2);
                //    profile.SetPreference("browser.download.dir", folderName);
                //    profile.SetPreference("browser.download.downloadDir", folderName);
                //    profile.SetPreference("browser.download.defaultFolder", folderName);
                //    profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "text/csv");
                //    _webDriver = new FirefoxDriver(profile);
                //    break;
                case Drivers.Chrome:
                    var options = new ChromeOptions();
                    options.AddArguments("--disable-extensions");
                    options.AddArguments("start-maximized");
                    //options.AddUserProfilePreference("download.default_directory", FileHelpers.CreateDirectory(""));
                    options.AddUserProfilePreference("credentials_enable_service", false);
                    options.AddUserProfilePreference("profile.password_manager_enabled", false);
                    try
                    {
                        // We use this to allow the tests to run via Jenkins
                        var service = ChromeDriverService.CreateDefaultService(@".\", "chromedriver.exe");
                        service.WhitelistedIPAddresses = @"""";
                        _webDriver = new ChromeDriver(service, options, TimeSpan.FromSeconds(180));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[INFO] {e.Message}");
                        _webDriver = new ChromeDriver(options);
                    }
                    break;
                //case Drivers.ChromeGrid:
                //    DesiredCapabilities capability = DesiredCapabilities.Chrome();
                //    capability.SetCapability(CapabilityType.BrowserName, "chrome");
                //    capability.SetCapability(CapabilityType.Platform, new Platform(PlatformType.Windows));

                //    _webDriver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub/"), capability, TimeSpan.FromSeconds(180));
                //    break;
                //case Drivers.PhantomJs:
                //    var phantomJsOptions = new PhantomJSOptions();
                //    phantomJsOptions.AddAdditionalCapability(CapabilityType.IsJavaScriptEnabled, true);

                //    _webDriver = new PhantomJSDriver(phantomJsOptions);

                //    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(100);
            _webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

            //try
            //{
            //    // RESIZING APPEARS TO CAUSE THE webdriver TO THROW AN ERROR FOR THE VERSION 2.31
            //    _webDriver.Manage().Window.Size = new Size(1400, 1000);
            //    _webDriver.Manage().Window.Maximize();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine($"[INFO] Window could not resize with the error {e.Message}");
            //}
        }

        public T Visit<T>(string identifier = null, Func<object, string> convert = null) where T : PageBuilder
        {
            var page = (T)Activator.CreateInstance(typeof(T), _webDriver);
            return Visit(page, identifier, convert);
        }

        public T Visit<T>(params Tuple<string, string>[] queryParams) where T : PageBuilder
        {
            var page = (T)Activator.CreateInstance(typeof(T), _webDriver);

            var url = (string)page.GetType().GetField("URL").GetValue(null);

            if (url != null)
            {
                url = url + "?" + String.Join("&", queryParams.Select(x => x.Item1 + "=" + x.Item2));
                _webDriver.Navigate().GoToUrl(url);
            }
            else
            {
                throw new Exception("[FAILED] No URL is available on the page " + typeof(T).FullName);
            }
            return page;
        }

        public T Visit<T>(T page, object identifier = null, Func<object, string> convert = null) where T : PageBuilder
        {
            var url = (string)page.GetType().GetField("URL").GetValue(null);
            if (url != null)
            {
                if (identifier != null)
                {
                    url = String.Format(url, convert != null ? convert(identifier) : identifier);
                }

                _webDriver.Navigate().GoToUrl(url);
            }
            else
            {
                throw new Exception("No URL is available on the page " + typeof(T).FullName);
            }

            CloseAlerts();

            return page;
        }

        public void Visit(string url)
        {
            _webDriver.Navigate().GoToUrl(url);
            Thread.Sleep(3000);
        }

        public T CurrentPage<T>() where T : PageBuilder
        {
            var page = (T)Activator.CreateInstance(typeof(T), _webDriver);
            return page;
        }

        #region Helpers

        // Scrolls the view port of the browser window to the top of the page
        public void ScrollWindowToTopOfThePage()
        {
            WaitForSeconds(2);
            ((IJavaScriptExecutor)_webDriver).ExecuteScript("scroll(0, 0)");
        }

        public void CloseBrowser()
        {
            _webDriver.Close();
        }

        public void CloseDriver()
        {
            _webDriver.Quit();
        }

        public string GetCurrentUrl()
        {
            return _webDriver.Url;
        }

        public void CloseAlerts()
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    var alert = _webDriver.SwitchTo().Alert();

                    if (alert != null)
                    {
                        alert.Accept();
                    }
                }
                catch (Exception) { }
            }
        }

        public string GetAlertText()
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    var alert = _webDriver.SwitchTo().Alert();

                    if (alert != null)
                    {
                        var alertText = alert.Text;
                        alert.Accept();
                        Console.WriteLine("[INFO] Alert text: " + alertText);
                        return alertText;
                    }
                }
                catch (Exception) { }
            }

            return "";
        }

        public void KillBrowserSession()
        {
            var nativeDriverQuit = Task.Factory.StartNew(() => _webDriver.Quit());
            if (!nativeDriverQuit.Wait(TimeSpan.FromSeconds(10)))
            {
                var currentProcessPid = Process.GetCurrentProcess().Id;
                foreach (var process in Process.GetProcesses())
                {
                    using (var mo = new ManagementObject("win32_process.handle='" + process.Id.ToString(CultureInfo.InvariantCulture) + "'"))
                    {
                        mo.Get();
                        var parentPid = Convert.ToInt32(mo["ParentProcessId"]);

                        if (parentPid == currentProcessPid)
                        {
                            Thread.Sleep(20000);
                            process.Kill();
                        }
                    }
                }
            }
        }

        public void TakeScreenshot(string screenshotFilepath)
        {
            try
            {
                var native = (ITakesScreenshot)_webDriver;

                native.GetScreenshot().SaveAsFile(screenshotFilepath, ScreenshotImageFormat.Png);

                Console.WriteLine("[SCREENSHOT] {0}", screenshotFilepath);
            }
            catch (Exception e)
            {
                Console.WriteLine("[WARNING] Could not take screenshot. Error: {0}", e.Message);
            }
            Console.WriteLine("");
        }

        public void RefreshPage()
        {
            _webDriver.Navigate().Refresh();
        }

        public void SetAttributeToPageObject(string attributeName, string newAttributeValue, string pageObjectString)
        {
            ((IJavaScriptExecutor)_webDriver).ExecuteScript(string.Format("arguments[0].setAttribute('{0}', '{1}')"
                , attributeName, newAttributeValue), _webDriver.FindElement(By.XPath(pageObjectString)));
        }

        public void WaitForSeconds(int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        public void PrintBrowserError()
        {
            var errors = _webDriver.Manage().Logs.GetLog(LogType.Browser);

            if (errors.Count > 0)
            {
                Console.WriteLine("[LIST OF BROWSER ERRORS]");

                var count = 1;
                foreach (var error in errors)
                {
                    Console.WriteLine($"[INFO] {count} => {error}");
                    count++;
                }

                Console.WriteLine("[END OF LIST]");
                Console.WriteLine("");
            }
        }

        #endregion
    }
}
