using System;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Utilities;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Actions;

public class TextFieldActions
{
    private readonly IWebDriver _driver;
    private readonly ILogger? _logger;

    public TextFieldActions(IWebDriver driver, ILogger? logger = null)
    {
        _driver = driver;
        _logger = logger;
    }

    public string SetText(ElementCandidate candidate, string textToEnter)
    {
        var input = candidate.Element;

        // 1. Eligibility validation
        if (!candidate.IsEnabled || !input.Enabled)
        {
            throw new InvalidElementStateException($"Input candidate '{candidate}' is disabled and cannot be modified.");
        }

        if (!candidate.IsVisible || !input.Displayed)
        {
            throw new ElementNotInteractableException($"Input candidate '{candidate}' is hidden and cannot be interacted with.");
        }

        WaitHelper.Retry(() =>
        {
            // Scroll into view if needed
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", input);

            // Clear existing value
            input.Clear();

            // Additional fallback clear via keyboard shortcut in case .Clear() was overridden by JS
            if (!string.IsNullOrEmpty(input.GetAttribute("value")))
            {
                input.SendKeys(Keys.Control + "a");
                input.SendKeys(Keys.Backspace);
            }

            // Send keys
            input.SendKeys(textToEnter);
            _logger?.LogInformation("Entered '{Text}' into candidate {Candidate}", textToEnter, candidate);
        });

        // Verification of entered text
        string actualValue = input.GetAttribute("value") ?? string.Empty;
        if (!string.Equals(actualValue, textToEnter, StringComparison.Ordinal))
        {
            _logger?.LogError("Value mismatch after SetText. Expected: '{Expected}', Actual: '{Actual}'", textToEnter, actualValue);
            throw new InvalidOperationException($"Value verification failed for input '{candidate}'. Expected: '{textToEnter}', Actual: '{actualValue}'.");
        }

        return actualValue;
    }
}

