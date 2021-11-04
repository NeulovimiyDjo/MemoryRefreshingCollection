using System;
using System.IO;
using DndBoard.SeleniumTestsBase.Helpers;
using OpenQA.Selenium.Chrome;

namespace DndBoard.SeleniumTestsBase.Fixtures
{
    public abstract class SetupClientsFixtureBase : IDisposable
    {
        private readonly ChromeDriver _driver;
        private readonly ChromeDriver _driver2;

        public string DataDir { get; }
        public ClientHelper ClientHelper { get; }
        public ClientHelper ClientHelper2 { get; }

        public abstract string SiteBaseAddress { get; }


        public SetupClientsFixtureBase()
        {
            string currentDir = Directory.GetCurrentDirectory();
            DataDir = Path.GetFullPath(Path.Combine(currentDir, "../../../../../TestData"));

            _driver = CreateNewDriver();
            ClientHelper = new ClientHelper(_driver);

            _driver2 = CreateNewDriver();
            ClientHelper2 = new ClientHelper(_driver2);
        }

        public void Dispose()
        {
            _driver.Dispose();
            _driver2.Dispose();
        }

        
        private ChromeDriver CreateNewDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            options.AddArgument("ignore-certificate-errors");

            ChromeDriver driver = new ChromeDriver(".", options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            return driver;
        }
    }
}
