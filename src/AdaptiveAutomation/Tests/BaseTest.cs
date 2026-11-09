using System;
using System.IO;
using AdaptiveAutomation.Browser;
using AdaptiveAutomation.Configuration;
using AdaptiveAutomation.Discovery;
using AdaptiveAutomation.Reporting;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Tests;

public abstract class BaseTest
{
    protected AppSettings Settings { get; private set; } = null!;
    protected BrowserManager BrowserManager { get; private set; } = null!;
    protected IWebDriver Driver => BrowserManager.Driver;
    protected ElementFinder ElementFinder { get; private set; } = null!;

    [SetUp]
    public virtual void SetUp()
    {
        Settings = ConfigurationLoader.Load();
        // Force headless for automated test runners unless explicitly specified
        Settings.Browser.Headless = true;
        BrowserManager = new BrowserManager(Settings);
        ElementFinder = new ElementFinder(Driver, Settings);
    }

    [TearDown]
    public virtual void TearDown()
    {
        try
        {
            var testContext = TestContext.CurrentContext;
            if (testContext.Result.Outcome.Status == TestStatus.Failed)
            {
                string testName = testContext.Test.Name;
                string? path = ScreenshotManager.Capture(Driver, testName);
                if (path != null)
                {
                    TestContext.AddTestAttachment(path, "Failure Screenshot");
                }
            }
        }
        finally
        {
            BrowserManager?.Dispose();
        }
    }

    protected string GetFixtureUrl(string fixtureFilename)
    {
        string currentDir = Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(currentDir, "fixtures", fixtureFilename);
        if (!File.Exists(fullPath))
        {
            // Check parent dirs if running from bin/Debug/net8.0
            string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "fixtures", fixtureFilename);
            if (File.Exists(altPath))
            {
                fullPath = Path.GetFullPath(altPath);
            }
            else
            {
                string rootCheck = Path.Combine(Directory.GetParent(currentDir)?.FullName ?? currentDir, "fixtures", fixtureFilename);
                if (File.Exists(rootCheck))
                {
                    fullPath = rootCheck;
                }
            }
        }
        return new Uri(fullPath).AbsoluteUri;
    }
}

