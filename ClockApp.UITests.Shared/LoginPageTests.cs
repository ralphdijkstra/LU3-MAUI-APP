using NUnit.Framework;

namespace ClockApp.UITests;

public class LoginPageTests : BaseTest
{
    [Test]
    public void LoginPage_DisplaysEmailPasswordAndLoginButton()
    {
        Assert.That(WaitForElement(UiTestIds.EmailEntry).Displayed, Is.True);
        Assert.That(WaitForElement(UiTestIds.PasswordEntry).Displayed, Is.True);
        Assert.That(WaitForElement(UiTestIds.LoginButton).Displayed, Is.True);
    }

    [Test]
    public void LoginWithInvalidCredentials_ShowsErrorMessage()
    {
        SetText(UiTestIds.EmailEntry, "invalid@test.com");
        SetText(UiTestIds.PasswordEntry, "wrong-password");
        Tap(UiTestIds.LoginButton);

        WaitForTextContaining(UiTestIds.LoginErrorMessage, "Login failed");
        Assert.That(GetText(UiTestIds.LoginErrorMessage), Does.Contain("Login failed").IgnoreCase);
    }

    [Test]
    public void LoginWithValidCredentials_ShowsClockInButton()
    {
        var email = Environment.GetEnvironmentVariable("CLOCKAPP_UI_TEST_EMAIL");
        var password = Environment.GetEnvironmentVariable("CLOCKAPP_UI_TEST_PASSWORD");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            Assert.Inconclusive("Set CLOCKAPP_UI_TEST_EMAIL and CLOCKAPP_UI_TEST_PASSWORD to run this test.");

        SetText(UiTestIds.EmailEntry, email);
        SetText(UiTestIds.PasswordEntry, password);
        Tap(UiTestIds.LoginButton);

        Assert.That(WaitForElement(UiTestIds.ClockTab, TimeSpan.FromSeconds(20)).Displayed, Is.True);
        Assert.That(WaitForElement(UiTestIds.ClockInButton, TimeSpan.FromSeconds(20)).Displayed, Is.True);
    }
}
