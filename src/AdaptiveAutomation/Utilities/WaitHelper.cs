using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AdaptiveAutomation.Utilities;

public static class WaitHelper
{
    public static void WaitForDocumentReady(IWebDriver driver, int timeoutSeconds = 15)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };

        wait.IgnoreExceptionTypes(typeof(NoSuchWindowException));

        try
        {
            wait.Until(d =>
            {
                try
                {
                    var js = (IJavaScriptExecutor)d;
                    var state = js.ExecuteScript("return document.readyState");
                    return "complete".Equals(state?.ToString(), StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            });
        }
        catch (WebDriverTimeoutException)
        {
            // Continue if document ready state wait times out
        }
    }

    public static T Retry<T>(Func<T> action, int maxAttempts = 3, int delayMs = 300)
    {
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (StaleElementReferenceException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts) Thread.Sleep(delayMs * attempt);
            }
            catch (ElementClickInterceptedException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts) Thread.Sleep(delayMs * attempt);
            }
            catch (ElementNotInteractableException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts) Thread.Sleep(delayMs * attempt);
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed without specific exception.");
    }

    public static void Retry(Action action, int maxAttempts = 3, int delayMs = 300)
    {
        Retry(() =>
        {
            action();
            return true;
        }, maxAttempts, delayMs);
    }
}

