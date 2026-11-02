using OpenQA.Selenium;

namespace GiftOfTheGivers.UITests;

// A.4: Selenium UI tests. The site must already be running at UI_TEST_BASE_URL.
// Users are kept in memory by the app, so each test registers its own account first.
public class SiteTests : IClassFixture<BrowserFixture>
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public SiteTests(BrowserFixture browser)
    {
        _driver = browser.Driver;
        _baseUrl = browser.BaseUrl;
    }

    // Scenario 1: employee registers, logs in and lands on the dashboard
    [Fact]
    public void EmployeeLogin_ShowsDashboard()
    {
        var username = Register("Employee");

        _driver.Navigate().GoToUrl($"{_baseUrl}/Register/Login");
        _driver.FindElement(By.Name("Username")).SendKeys(username);
        _driver.FindElement(By.Name("Password")).SendKeys("P@ssw0rd!");
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        Assert.Contains("/Employee/Dashboard", _driver.Url);
        Assert.Equal("Good morning, team.", _driver.FindElement(By.Id("dashboard-title")).Text);
    }

    [Fact]
    public void EmployeeLogin_WrongPassword_ShowsError()
    {
        var username = Register("Employee");

        _driver.Navigate().GoToUrl($"{_baseUrl}/Register/Login");
        _driver.FindElement(By.Name("Username")).SendKeys(username);
        _driver.FindElement(By.Name("Password")).SendKeys("not-the-password");
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        Assert.Contains("Invalid credentials", _driver.FindElement(By.CssSelector(".auth-alert-error")).Text);
    }

    // Scenario 2: donor logs in and makes an anonymous donation (no name entered)
    [Fact]
    public void DonorAnonymousDonation_ShowsTaxCertificate()
    {
        var username = Register("Donor");

        _driver.Navigate().GoToUrl($"{_baseUrl}/Register/Dlogin");
        _driver.FindElement(By.Name("Username")).SendKeys(username);
        _driver.FindElement(By.Name("Password")).SendKeys("P@ssw0rd!");
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        Assert.Contains("/Donor/Donate", _driver.Url);

        _driver.FindElement(By.Name("DonationAmount")).SendKeys("100");
        _driver.FindElement(By.Name("Currency")).SendKeys("ZAR");
        _driver.FindElement(By.Name("Email")).SendKeys("selenium@example.com");
        _driver.FindElement(By.XPath("//button[normalize-space()='Donate Now']")).Click();

        var certificate = _driver.FindElement(By.CssSelector(".cert")).Text;
        Assert.Contains("Anonymous", certificate);
        Assert.Contains("ZAR 100.00", certificate);
        // GOTG-... from the Azure Function, or TX-... if the function is not running
        Assert.Matches(@"(GOTG|TX)-", certificate);
    }

    [Fact]
    public void EmployeePostsProjectUpdate_ShowsSuccessMessage()
    {
        var username = Register("Employee");

        _driver.Navigate().GoToUrl($"{_baseUrl}/Register/Login");
        _driver.FindElement(By.Name("Username")).SendKeys(username);
        _driver.FindElement(By.Name("Password")).SendKeys("P@ssw0rd!");
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        _driver.FindElement(By.Id("Title")).SendKeys("Selenium update");
        _driver.FindElement(By.Id("Message")).SendKeys("Posted by an automated UI test.");
        _driver.FindElement(By.CssSelector("form.dashboard-form button[type='submit']")).Click();

        Assert.Contains("Update posted successfully", _driver.FindElement(By.CssSelector(".dashboard-alert")).Text);
    }

    private string Register(string role)
    {
        var username = $"ui{Guid.NewGuid():N}"[..16];

        _driver.Navigate().GoToUrl($"{_baseUrl}/Register/Register");
        _driver.FindElement(By.Name("Fullname")).SendKeys("UI Test User");
        _driver.FindElement(By.Name("Username")).SendKeys(username);
        _driver.FindElement(By.Name("Email")).SendKeys("ui@example.com");
        _driver.FindElement(By.Name("Password")).SendKeys("P@ssw0rd!");
        _driver.FindElement(By.Name("role")).SendKeys(role);
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        return username;
    }
}
