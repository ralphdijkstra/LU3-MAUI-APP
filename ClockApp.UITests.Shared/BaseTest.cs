using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;

namespace ClockApp.UITests;

public abstract class BaseTest
{
    protected AppiumDriver App => AppiumSetup.App;

    protected AppiumElement FindUIElement(string id) => App is WindowsDriver
        ? App.FindElement(MobileBy.AccessibilityId(id))
        : App.FindElement(MobileBy.Id(id));

    protected AppiumElement WaitForElement(string id, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(App, timeout ?? TimeSpan.FromSeconds(10));

        return wait.Until(_ => FindUIElement(id));
    }

    protected void SetText(string id, string text)
    {
        var element = WaitForElement(id);
        element.Click();
        element.Clear();
        element.SendKeys(text);
    }

    protected void Tap(string id) => WaitForElement(id).Click();

    protected string GetText(string id) => WaitForElement(id).Text;

    protected void WaitForTextContaining(string id, string expectedSubstring, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(App, timeout ?? TimeSpan.FromSeconds(15));

        wait.Until(_ =>
        {
            var text = FindUIElement(id).Text;

            return text.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase);
        });
    }
}
