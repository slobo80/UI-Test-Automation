using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Threading;

namespace UI.Test
{
    /// <summary>
    /// Suite of tests verifying that the first result on the search result page is "Seattle Code Camp" when we search for "Seattle Code Camp"
    /// </summary>
    [TestClass]
    public class SearchResultsTest
    {
        private IWebDriver driver;

        /// <summary>
        /// This is the right place to initalize the driver so that every test is independent and starts clean
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            var options = new ChromeOptions();

            // Using Chrome because of its marketshare and due to PhantomJS being abandoned by its maintainer.
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3); // Correct place to set it.
            driver.Navigate().GoToUrl("http://www.google.com/");
        }

        /// <summary>
        /// Important to quit and dispose the driver to avoid memory leaks
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            driver.Quit();
            driver.Dispose();
        }

        /// <summary>
        /// It also takes a screenshot of the search result page. 
        /// Very useful for troubleshooting and understand what the page under test looks like.
        /// 
        /// What is bad about the test:
        /// - Unreliable due to page load time flactuation. We are asserting on the text before without waiting for the page to complete loading. 
        ///     This is especially pronounced on the sites that make AJAX calls.
        /// - Fragile due to tight coupling to HTML
        /// 
        /// Recommendation:
        /// - Wait for page or its part to complete loading
        /// - Implement PageObject patttern
        /// - Take a screenshot only if the test fail
        /// </summary>
        [TestMethod]
        public void BasicTest()
        {
            IWebElement element = driver.FindElement(By.Name("q")); // Move By.Name("q") to a PageObject
            Thread.Sleep(TimeSpan.FromSeconds(3));
            element.SendKeys("Seattle Code Camp");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            element.Submit();
            Thread.Sleep(TimeSpan.FromSeconds(3));

            IWebElement result = driver.FindElement(By.CssSelector("h3.r a")); // Wait again for page to finish loading

            Screenshot screenShot = ((ITakesScreenshot)driver).GetScreenshot(); // Do this only in case test fails
            screenShot.SaveAsFile("result.png", ScreenshotImageFormat.Png);

            Assert.AreEqual("Seattle Code Camp", result.Text);
        }

        /// <summary>
        /// Reliable variation of BasicTest
        /// 
        /// What is bad about the test:
        /// - Fragile due to tight coupling to HTML
        /// 
        /// Recommendation:
        /// - Implement PageObject patttern
        /// - Take screenshots only if tests fail
        /// </summary>
        [TestMethod]
        public void GoodTest()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("q"))); // Good! Waiting for page to load before locating the element later.

            IWebElement element = driver.FindElement(By.Name("q"));
            element.SendKeys("Seattle Code Camp");
            element.Submit();

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("h3.r a")));
            IWebElement result = driver.FindElement(By.CssSelector("h3.r a"));

            Screenshot screenShot = ((ITakesScreenshot)driver).GetScreenshot();
            screenShot.SaveAsFile("result.png", ScreenshotImageFormat.Png);

            Assert.AreEqual("Seattle Code Camp", result.Text);
        }

        /// <summary>
        /// Reliable and decoupled variation of BasicTest
        /// 
        /// Recommendation:
        /// - Take screenshots only if tests fail
        /// </summary>
        [TestMethod]
        public void BetterTest()
        {
            var page = new SearchResultPageObject(driver); // Good! Decouples test from HTML

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.Until(ExpectedConditions.ElementIsVisible(page.BySearchBox));

            IWebElement element = driver.FindElement(page.BySearchBox);
            element.SendKeys("Seattle Code Camp");
            element.Submit();

            wait.Until(ExpectedConditions.ElementIsVisible(page.ByFirstSearchResult));
            IWebElement result = driver.FindElement(page.ByFirstSearchResult);

            Screenshot screenShot = ((ITakesScreenshot)driver).GetScreenshot();
            screenShot.SaveAsFile("result.png", ScreenshotImageFormat.Png);

            Assert.AreEqual("Seattle Code Camp", result.Text);
        }

        /// <summary>
        /// Reliable, decoupled, Idiomatic, maintanable, readable, *idiomatic* variation of BasicTest
        /// 
        /// Recommendation:
        /// - Take screenshots only if tests fail
        /// </summary>
        [TestMethod]
        public void BestestTest()
        {
            var page = new SearchResultPageObject(driver);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.Until(ExpectedConditions.ElementIsVisible(page.BySearchBox));

            new Actions(driver) // Good! Fluent interface makes it more readable and maintanable
                .MoveToElement(page.SearchBox)
                .SendKeys("Seattle Code Camp")
                .SendKeys(Keys.Enter)
               .Perform();

            wait.Until(ExpectedConditions.ElementIsVisible(page.ByFirstSearchResult));

            Screenshot screenShot = ((ITakesScreenshot)driver).GetScreenshot();
            screenShot.SaveAsFile("result.png", ScreenshotImageFormat.Png);

            Assert.AreEqual("Seattle Code Camp", page.FirstSearchResult.Text);
        }

        /// <summary>
        /// This test shows how to verify that element is not present on the page. 
        /// A common use case would be where we want to verify that a dialog has been dismissed successfuly.
        /// 
        /// What is bad about the test:
        /// - It throws an exception and therefore never hits the Assert
        /// - Throws an exception we are not throwing. The driver throws the exception.
        /// 
        /// Recommendation:
        /// - Only expect exceptions you are throwing explicitly
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NoSuchElementException))]
        public void BadElementNotPresentTest()
        {
            var oneElement = driver.FindElement(By.Id("missingElementId")); // BAD!! Throws NoSuchElementException
            Assert.IsNull(oneElement); // This never gets executed
        }

        /// <summary>
        /// Like the BadElementNotPresentTest this test also shows how to verify that element is not present on the page. 
        /// 
        /// What is bad about the test:
        /// - Very sloooow as it needs to traverse entire DOM tree to find all the elements that satisfy the condition
        /// 
        /// Recommendation:
        /// - Look for other ways to validate your code, i.e. integration test or use of telemetry.
        /// </summary>
        [TestMethod]
        public void BetterElementNotPresentTest()
        {
            var manyElements = driver.FindElements(By.Id("missingElementId")); // GOOD, returns an empty collection
            Assert.AreEqual(0, manyElements.Count);
        }
    }
}
