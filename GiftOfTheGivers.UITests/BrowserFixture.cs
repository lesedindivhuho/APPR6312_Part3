using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GiftOfTheGivers.UITests;

// One Chrome window per test class. Selenium Manager downloads the matching chromedriver automatically.
//   UI_TEST_BASE_URL  address of the running site (default http://localhost:5171)
//   UI_TEST_HEADLESS  "false" to watch the browser (default true, which the pipeline needs)
public class BrowserFixture : IDisposable
{
    public IWebDriver Driver { get; }
    public string BaseUrl { get; }

    public BrowserFixture()
    {
        BaseUrl = (Environment.GetEnvironmentVariable("UI_TEST_BASE_URL") ?? "http://localhost:5171").TrimEnd('/');

        var options = new ChromeOptions();
        if (!string.Equals(Environment.GetEnvironmentVariable("UI_TEST_HEADLESS"), "false", StringComparison.OrdinalIgnoreCase))
        {
            options.AddArgument("--headless=new");
        }
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1400,1000");
        options.AddArgument("--ignore-certificate-errors");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}
