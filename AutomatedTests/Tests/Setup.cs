using AutomatedTests.Browser.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace AutomatedTests.Tests
{
    public class Setup : IDisposable
    {
        public Browser.Browser Browser;
        public IConfiguration Config;

        public Setup(Drivers driver)
        {
            Browser = new Browser.Browser(Drivers.Chrome);

            Config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("config.json")
                .Build();
        }

        public void Dispose()
        {
            Browser.CloseBrowser();
            Browser.CloseDriver();
        }
    }
}
