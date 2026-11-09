using System;
using AdaptiveAutomation.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace AdaptiveAutomation.Browser;

public static class WebDriverFactory
{
    public static IWebDriver CreateDriver(AppSettings settings)
    {
        string browserName = settings.Browser.Name.Trim().ToLowerInvariant();

        IWebDriver driver = browserName switch
        {
            "edge" => CreateEdgeDriver(settings.Browser.Headless),
            _ => CreateChromeDriver(settings.Browser.Headless)
        };

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(settings.Execution.TimeoutSeconds);
        driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);

        return driver;
    }

    private static ChromeDriver CreateChromeDriver(bool headless)
    {
        var options = new ChromeOptions();

        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-search-engine-choice-screen");
        options.AddArgument("--window-size=1920,1080");

        return new ChromeDriver(options);
    }

    private static EdgeDriver CreateEdgeDriver(bool headless)
    {
        var options = new EdgeOptions();

        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1920,1080");

        return new EdgeDriver(options);
    }
}

