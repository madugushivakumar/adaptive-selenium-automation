using System;
using System.IO;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Reporting;

public static class ScreenshotManager
{
    public static string? Capture(IWebDriver driver, string testName)
    {
        if (driver is not ITakesScreenshot screenshotDriver)
        {
            return null;
        }

        try
        {
            var screenshot = screenshotDriver.GetScreenshot();
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "screenshots", "failures");
            Directory.CreateDirectory(dir);

            string filePath = Path.Combine(dir, $"{safeName}_{timestamp}.png");
            screenshot.SaveAsFile(filePath);
            return filePath;
        }
        catch
        {
            return null;
        }
    }
}

