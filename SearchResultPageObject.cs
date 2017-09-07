using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace UI.Test
{
    /// <summary>
    /// PageObject representing page. Can be reused by multiple test classes. 
    /// A single page component resued across pages can be represented by a single page object.
    /// </summary>
    public class SearchResultPageObject
    {
        [FindsBy(How = How.CssSelector, Using = "h3.r a")]
        public IWebElement FirstSearchResult { get; set; }

        [FindsBy(How = How.Name, Using = "q")]
        public IWebElement SearchBox { get; set; }

        public By ByFirstSearchResult => By.CssSelector("h3.r a");

        // Preferred method
        public By BySearchBox => By.Name("q");

        private IWebDriver driver;

        public SearchResultPageObject(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(this.driver, this);
        }
    }
}
