using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace WorkerService1.Utilities;

public static class HHCookieService
{
    public static CookieContainer GetHHCookie()
    {
        EdgeOptions edgeOptions = new() { PageLoadStrategy = PageLoadStrategy.Normal };
        edgeOptions.AddArgument("--headless=new");
        EdgeDriver driver = new(edgeOptions);
        driver.Navigate().GoToUrl("https://hhey.shaphar.com/");
        var userName = driver.FindElement(By.XPath("//*[@id=\"userName\"]"));
        userName.SendKeys("xzhx");
        var password = driver.FindElement(By.XPath("//*[@id=\"password\"]"));
        password.SendKeys("123456");
        var logInButton = driver.FindElement(By.XPath("//*[@id=\"btn_sub\"]"));
        logInButton.Click();
        Thread.Sleep(1000);
        CookieContainer cookieContainer = new();
        foreach (var cookie in driver.Manage().Cookies.AllCookies)
        {
            cookieContainer.Add(new System.Net.Cookie()
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Domain = cookie.Domain,
                Path = cookie.Path,
                Secure = cookie.Secure,
            });
        }
        driver.Quit();
        return cookieContainer;
    }
}
