using System;
using System.IO;
using AdaptiveAutomation.Configuration;
using AdaptiveAutomation.Utilities;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Browser;

public class BrowserManager : IDisposable
{
    private readonly AppSettings _settings;
    private readonly ILogger? _logger;
    private IWebDriver? _driver;
    private bool _disposed;

    public BrowserManager(AppSettings settings, ILogger? logger = null)
    {
        _settings = settings;
        _logger = logger;
    }

    public IWebDriver Driver => _driver ??= WebDriverFactory.CreateDriver(_settings);

    public void NavigateTo(string url)
    {
        _logger?.LogInformation("Navigating to URL: {Url}", url);
        Driver.Navigate().GoToUrl(url);
        WaitHelper.WaitForDocumentReady(Driver, _settings.Execution.TimeoutSeconds);
    }

    public string? CaptureScreenshot(string prefix)
    {
        if (_driver is not ITakesScreenshot screenshotDriver)
        {
            return null;
        }

        try
        {
            var screenshot = screenshotDriver.GetScreenshot();
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");
            string safePrefix = string.Join("_", prefix.Split(Path.GetInvalidFileNameChars()));
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "screenshots", "failures");
            Directory.CreateDirectory(dir);

            string filePath = Path.Combine(dir, $"{safePrefix}_{timestamp}.png");
            screenshot.SaveAsFile(filePath);
            _logger?.LogInformation("Screenshot saved: {Path}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to capture screenshot.");
            return null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
                _logger?.LogInformation("WebDriver session disposed cleanly.");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error while disposing WebDriver session.");
            }
            finally
            {
                _driver = null;
                _disposed = true;
            }
        }

        GC.SuppressFinalize(this);
    }
}

