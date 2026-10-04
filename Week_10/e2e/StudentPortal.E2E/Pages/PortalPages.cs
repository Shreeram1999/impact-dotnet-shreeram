using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using StudentPortal.E2E.Infrastructure;

namespace StudentPortal.E2E.Pages;

// Page objects: each class knows how ONE screen of the SPA is built (ids,
// labels, button text), so the tests read as user journeys and a markup
// change is fixed in one place. The locators are the same ids/labels the
// React components use for accessibility.

public class LoginPage(BrowserSession browser)
{
    public static readonly By Heading = By.XPath("//h2[normalize-space()='Log in']");

    public LoginPage Open()
    {
        browser.Open("/login");
        browser.Find(Heading);
        return this;
    }

    public bool IsShown => browser.Driver.FindElements(Heading).Count == 1;

    public string UsernameValue => browser.Find(By.Id("username")).GetAttribute("value") ?? string.Empty;

    public bool HasNotice(string text) =>
        browser.Driver.FindElements(By.CssSelector(".notice")).Any(n => n.Text.Contains(text));

    public StudentsPage LogInAs(string username, string password)
    {
        browser.Find(By.Id("username")).ReplaceText(username);
        browser.Find(By.Id("password")).SendKeys(password);
        browser.Find(By.XPath("//button[normalize-space()='Log in']")).Click();
        return new StudentsPage(browser).WaitUntilLoaded();
    }
}

public class RegisterPage(BrowserSession browser)
{
    public RegisterPage Open()
    {
        browser.Open("/register");
        browser.Find(By.XPath("//h2[normalize-space()='Create an account']"));
        return this;
    }

    public LoginPage Register(string name, string dateOfBirth, string designation, string email, string password)
    {
        browser.Find(By.Id("name")).SendKeys(name);
        SetDate(browser.Find(By.Id("dateOfBirth")), dateOfBirth);
        new SelectElement(browser.Find(By.Id("designation"))).SelectByValue(designation);
        browser.Find(By.Id("email")).SendKeys(email);
        browser.Find(By.Id("password")).SendKeys(password);

        var submit = browser.Find(By.XPath("//button[normalize-space()='Register']"));
        browser.WaitFor(_ => submit.Enabled, "Register to be enabled");
        submit.Click();

        browser.Find(LoginPage.Heading);
        return new LoginPage(browser);
    }

    // Typing into <input type="date"> is locale-dependent, so set the value
    // the way React expects (native setter + input event).
    private void SetDate(IWebElement input, string isoDate) =>
        ((IJavaScriptExecutor)browser.Driver).ExecuteScript(
            """
            const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value').set;
            setter.call(arguments[0], arguments[1]);
            arguments[0].dispatchEvent(new Event('input', { bubbles: true }));
            arguments[0].dispatchEvent(new Event('blur', { bubbles: true }));
            """, input, isoDate);
}

public class StudentsPage(BrowserSession browser)
{
    public StudentsPage WaitUntilLoaded()
    {
        browser.Find(By.Id("search"));
        browser.WaitFor(d => d.FindElements(By.CssSelector(".loading")).Count == 0);
        return this;
    }

    public string NavBarName => browser.Find(By.CssSelector(".navbar .who")).Text;
    public string NavBarRole => browser.Find(By.CssSelector(".navbar .role-badge")).Text;

    public bool HasAddButton => browser.Driver.FindElements(By.XPath("//button[normalize-space()='Add student']")).Count > 0;
    public int EditButtonCount => browser.Driver.FindElements(By.XPath("//button[starts-with(@aria-label,'Edit ')]")).Count;
    public int DeleteButtonCount => browser.Driver.FindElements(By.XPath("//button[starts-with(@aria-label,'Delete ')]")).Count;
    public bool ShowsReadOnlyHint => browser.Driver.FindElements(By.XPath("//*[normalize-space()='You have read-only access.']")).Count > 0;
    public int RowCount => browser.Driver.FindElements(By.CssSelector("table.students tbody tr")).Count;

    public IWebElement? RowFor(string name) =>
        browser.Driver.FindElements(By.XPath($"//table[contains(@class,'students')]//tr[td[1][normalize-space()='{name}']]")).FirstOrDefault();

    public string CellText(string name, int column) => RowFor(name)!.FindElements(By.TagName("td"))[column].Text;

    public void Search(string text)
    {
        browser.Find(By.Id("search")).ReplaceText(text);
    }

    public void AddStudent(string name, int age, string roll, string email, int score)
    {
        browser.Find(By.XPath("//button[normalize-space()='Add student']")).Click();
        FillForm(name, age, roll, email, score);
        Save();
        WaitForNotice($"Added {name}.");
    }

    public void EditScore(string name, int newScore)
    {
        browser.Find(By.CssSelector($"button[aria-label='Edit {name}']")).Click();
        browser.Find(By.Id("student-score")).ReplaceText(newScore.ToString());
        Save();
        WaitForNotice($"Saved {name}.");
    }

    public void Delete(string name)
    {
        browser.Find(By.CssSelector($"button[aria-label='Delete {name}']")).Click();
        browser.Wait.Until(ExpectedConditions.AlertIsPresent()).Accept(); // window.confirm
        WaitForNotice($"Deleted {name}.");
    }

    public LoginPage LogOut()
    {
        browser.Find(By.XPath("//button[normalize-space()='Log out']")).Click();
        browser.Find(LoginPage.Heading);
        return new LoginPage(browser);
    }

    private void FillForm(string name, int age, string roll, string email, int score)
    {
        browser.Find(By.Id("student-name")).SendKeys(name);
        browser.Find(By.Id("student-age")).SendKeys(age.ToString());
        browser.Find(By.Id("student-rollNumber")).SendKeys(roll);
        browser.Find(By.Id("student-email")).SendKeys(email);
        browser.Find(By.Id("student-score")).SendKeys(score.ToString());
    }

    private void Save()
    {
        var save = browser.Find(By.XPath("//button[normalize-space()='Save']"));
        browser.WaitFor(_ => save.Enabled, "Save to be enabled");
        save.Click();
    }

    private void WaitForNotice(string text) =>
        browser.WaitFor(d => d.FindElements(By.CssSelector(".notice")).Any(n => n.Text == text));
}

// Selenium.Support no longer ships ExpectedConditions; this is the one
// condition the suite needs.
internal static class ExpectedConditions
{
    public static Func<IWebDriver, IAlert> AlertIsPresent() => driver =>
    {
        try { return driver.SwitchTo().Alert(); }
        catch (NoAlertPresentException) { return null!; }
    };
}

internal static class ElementExtensions
{
    // IWebElement.Clear() empties the DOM value behind React's back (no
    // input event), so React keeps the old value and the next keystrokes
    // are appended to it ("55" + "88" = "5588"). Select-all + Delete is what
    // a user does, and React sees every step.
    public static void ReplaceText(this IWebElement element, string text)
    {
        element.SendKeys(Keys.Control + "a");
        element.SendKeys(Keys.Delete);
        element.SendKeys(text);
    }
}
