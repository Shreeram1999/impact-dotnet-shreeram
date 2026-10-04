using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace StudentPortal.E2E.Infrastructure;

// One real browser per test. The SPA keeps the JWT in memory only, so a
// fresh browser is a guaranteed logged-out start, and tests can't leak a
// session into each other.
//
// If the portal isn't running, the test is SKIPPED with a clear reason (run
// run-all-tests.ps1, which starts it) - never silently passed.
public sealed class BrowserSession : IDisposable
{
    private static readonly HttpClient Probe = new() { Timeout = TimeSpan.FromSeconds(3) };
    private static readonly Lazy<string?> PortalProblem = new(CheckPortal);

    public IWebDriver Driver { get; }
    public WebDriverWait Wait { get; }

    public BrowserSession()
    {
        Skip.If(PortalProblem.Value is not null, PortalProblem.Value);

        Driver = E2ESettings.Browser == "edge" ? CreateEdge() : CreateChrome();
        Driver.Manage().Window.Size = new System.Drawing.Size(1280, 900);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    public void Open(string path) => Driver.Navigate().GoToUrl(E2ESettings.WebUrl + path);

    // On a timeout, say what the page DID show and save a screenshot to
    // TestResults/e2e-screenshots, so a failure explains itself.
    public IWebElement Find(By by)
    {
        try
        {
            return Wait.Until(d => d.FindElement(by));
        }
        catch (WebDriverTimeoutException ex)
        {
            throw Explain($"Timed out waiting for {by}", ex);
        }
    }

    public void WaitFor(Func<IWebDriver, bool> condition, string description = "a condition")
    {
        try
        {
            Wait.Until(condition);
        }
        catch (WebDriverTimeoutException ex)
        {
            throw Explain($"Timed out waiting for {description}", ex);
        }
    }

    private Exception Explain(string what, Exception inner)
    {
        var text = Driver.FindElement(By.TagName("body")).Text.Replace('\n', ' ');
        var folder = Path.Combine(AppContext.BaseDirectory, "e2e-screenshots");
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, $"{DateTime.Now:HHmmssfff}.png");
        ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(file);
        return new WebDriverException(
            $"{what} at {Driver.Url}. Visible text: \"{text[..Math.Min(text.Length, 500)]}\". Screenshot: {file}", inner);
    }

    public void Dispose() => Driver?.Quit();

    // Selenium Manager (bundled with Selenium 4) downloads a driver that
    // matches the installed browser, so no chromedriver.exe is checked in.
    private static IWebDriver CreateChrome()
    {
        var options = new ChromeOptions();
        if (E2ESettings.Headless) options.AddArgument("--headless=new");
        options.AddArgument("--disable-search-engine-choice-screen");
        DisablePasswordManager(options);
        return new ChromeDriver(options);
    }

    private static IWebDriver CreateEdge()
    {
        var options = new EdgeOptions();
        if (E2ESettings.Headless) options.AddArgument("--headless=new");
        DisablePasswordManager(options);
        return new EdgeDriver(options);
    }

    // After a form login, Chromium's password manager pops up "save
    // password?" / "this password was found in a data breach" (the demo
    // passwords are deliberately simple). That bubble swallows the next native
    // click: WebDriver reports success but the page never receives the event.
    // A test browser has no use for saved passwords, so switch it all off.
    private static void DisablePasswordManager(OpenQA.Selenium.Chromium.ChromiumOptions options)
    {
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("profile.password_manager_leak_detection", false);
        options.AddArgument("--disable-features=PasswordLeakDetection,PasswordCheck,PasswordManagerOnboarding");
    }

    private static string? CheckPortal()
    {
        foreach (var url in new[] { E2ESettings.WebUrl, E2ESettings.ApiUrl + "/health" })
        {
            try
            {
                Probe.GetAsync(url).GetAwaiter().GetResult().EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return $"Portal not reachable at {url} ({ex.GetType().Name}). Start it with run-all-tests.ps1, or the API + `npm run dev`.";
            }
        }

        return null;
    }
}
