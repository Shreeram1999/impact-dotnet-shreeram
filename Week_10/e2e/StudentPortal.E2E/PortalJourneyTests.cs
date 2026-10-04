using System.Net;
using System.Net.Http.Json;
using OpenQA.Selenium;
using StudentPortal.E2E.Infrastructure;
using StudentPortal.E2E.Pages;

namespace StudentPortal.E2E;

// Week 10 (Task 10.7) - the Week 9 browser journeys, now against the
// COMPOSED system: React -> YARP gateway -> Identity / Academics / Reporting,
// each with its own database.
// Unit tests prove each piece; these prove the pieces work together for a
// real user in a real browser.
public class PortalJourneyTests
{
    private static string Unique(string prefix) => $"{prefix}{DateTime.UtcNow:HHmmssfff}";

    // Task 9.5 - smoke: the browser launches and an anonymous visitor lands on login.
    [SkippableFact]
    public void Smoke_AnonymousVisitor_IsSentToTheLoginPage()
    {
        using var browser = new BrowserSession();

        browser.Open("/students");

        browser.Find(LoginPage.Heading);
        Assert.EndsWith("/login", browser.Driver.Url);
        Assert.Equal("Student Portal", browser.Driver.Title);
    }

    // Task 9.6 / 9.7 (1) - register + login, then validate the JWT session:
    // the authenticated view shows who the token says we are, and the
    // student list loaded through the token-carrying API call.
    [SkippableFact]
    public void RegisterThenLogin_ShowsAnAuthenticatedReadOnlyView()
    {
        using var browser = new BrowserSession();
        var email = $"{Unique("e2e")}@school.example";

        var login = new RegisterPage(browser).Open()
            .Register("E2E Student", "2005-05-20", "Student", email, "Passw0rd1");

        Assert.True(login.HasNotice("Registration successful"));
        Assert.Equal(email, login.UsernameValue);

        var students = login.LogInAs(email, "Passw0rd1");

        Assert.Equal("E2E Student", students.NavBarName);
        Assert.Equal("Student", students.NavBarRole);
        Assert.True(students.RowCount >= 1, "The list should load through the authenticated API call.");
        Assert.False(students.HasAddButton);
    }

    // Task 9.7 (2) - a Teacher creates, edits and deletes a student
    // entirely through the browser.
    [SkippableFact]
    public void Teacher_FullCrud_ThroughTheBrowser()
    {
        using var browser = new BrowserSession();
        var roll = Unique("E");
        var name = $"E2E Pupil {roll}";

        var students = new LoginPage(browser).Open().LogInAs(E2ESettings.TeacherUsername, E2ESettings.TeacherPassword);
        Assert.Equal("Teacher", students.NavBarRole);

        students.AddStudent(name, 17, roll, $"{roll}@school.example", 55);
        Assert.NotNull(students.RowFor(name));
        Assert.Equal("55", students.CellText(name, 4));

        students.EditScore(name, 88);
        Assert.Equal("88", students.CellText(name, 4));

        students.Search(roll);
        browser.WaitFor(_ => students.RowCount == 1);

        students.Delete(name);
        browser.WaitFor(_ => students.RowFor(name) is null);
    }

    // Task 9.7 (3) - a Student is read-only: no Add, no Edit, no Delete.
    [SkippableFact]
    public void Student_IsReadOnly()
    {
        using var browser = new BrowserSession();

        var students = new LoginPage(browser).Open().LogInAs(E2ESettings.StudentUsername, E2ESettings.StudentPassword);

        Assert.Equal("Student", students.NavBarRole);
        Assert.True(students.RowCount >= 1);
        Assert.False(students.HasAddButton);
        Assert.Equal(0, students.EditButtonCount);
        Assert.Equal(0, students.DeleteButtonCount);
        Assert.True(students.ShowsReadOnlyHint);
    }

    // Task 9.7 (4) - logout ends the session: back on the login page, and
    // navigating to /students INSIDE the running app (no page reload, so an
    // in-memory session that survived logout would still be there) is
    // bounced straight back to login.
    [SkippableFact]
    public void Logout_EndsTheSession_AndTheListCannotBeReopened()
    {
        using var browser = new BrowserSession();
        var students = new LoginPage(browser).Open().LogInAs(E2ESettings.TeacherUsername, E2ESettings.TeacherPassword);

        var login = students.LogOut();
        Assert.True(login.IsShown);

        ((IJavaScriptExecutor)browser.Driver).ExecuteScript(
            "history.pushState({}, '', '/students'); dispatchEvent(new PopStateEvent('popstate'));");

        browser.WaitFor(d => d.Url.EndsWith("/login"), "the redirect back to /login");
        browser.Find(LoginPage.Heading);
        Assert.Empty(browser.Driver.FindElements(By.Id("search")));
    }

    // Task 9.8 - negative E2E. The UI hides write buttons from a Student,
    // but hiding protects nothing, so this goes around the UI: from INSIDE
    // the logged-in Student's browser page (same origin, real CORS, real
    // token), it calls the Teacher-only endpoint directly. The SERVER must
    // refuse it.
    [SkippableFact]
    public async Task StudentWriteAttempt_IsRejectedByTheServer_EndToEnd()
    {
        using var browser = new BrowserSession();
        var students = new LoginPage(browser).Open().LogInAs(E2ESettings.StudentUsername, E2ESettings.StudentPassword);
        Assert.False(students.HasAddButton); // the UI hides it...
        var roll = Unique("N");

        var status = ((IJavaScriptExecutor)browser.Driver).ExecuteAsyncScript(
            """
            const [api, roll, done] = arguments;
            (async () => {
              const login = await fetch(api + '/identity/api/auth/login', {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username: 'student1', password: 'Student@123' }) });
              const { token } = await login.json();
              const write = await fetch(api + '/academics/api/students', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', Authorization: 'Bearer ' + token },
                body: JSON.stringify({ name: 'Sneaky', age: 20, rollNumber: roll, email: roll + '@x.example', score: 99 }) });
              done(write.status);
            })().catch(e => done('error: ' + e));
            """, E2ESettings.ApiUrl, roll);

        Assert.Equal(403L, status); // ...and Academics enforces it, behind the gateway.

        // Nothing was written (checked as a Teacher, through the gateway).
        using var http = new HttpClient();
        var login = await http.PostAsJsonAsync($"{E2ESettings.ApiUrl}/identity/api/auth/login",
            new { username = E2ESettings.TeacherUsername, password = E2ESettings.TeacherPassword });
        var token = (await login.Content.ReadFromJsonAsync<Dictionary<string, object>>())!["token"].ToString();
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var matches = await http.GetFromJsonAsync<List<object>>($"{E2ESettings.ApiUrl}/academics/api/students/search?name=Sneaky");
        Assert.Empty(matches!);
    }

    // Week 10 (Task 10.8) - the reporting view: the Reports page pulls an
    // enrollment summary from the Reporting service (EF DB First) through
    // the gateway, in the same session as the Academics data.
    [SkippableFact]
    public void ReportingView_ShowsTheEnrollmentSummary_AndTermsCanBeSwitched()
    {
        using var browser = new BrowserSession();
        new LoginPage(browser).Open().LogInAs(E2ESettings.StudentUsername, E2ESettings.StudentPassword);

        browser.Find(By.LinkText("Reports")).Click();

        browser.Find(By.XPath("//h2[normalize-space()='Enrollment summary - Monsoon 2026']"));
        Assert.NotNull(browser.Find(By.XPath("//table[@aria-label='Enrollment summary']//td[normalize-space()='CS201']")));

        new OpenQA.Selenium.Support.UI.SelectElement(browser.Find(By.Id("term"))).SelectByText("Spring 2026");

        browser.Find(By.XPath("//h2[normalize-space()='Enrollment summary - Spring 2026']"));
        var mathRow = browser.Find(By.XPath("//tr[td[1][normalize-space()='MATH101']]"));
        Assert.Contains("66.5", mathRow.Text);
        Assert.Contains("75%", mathRow.Text);
    }
}
