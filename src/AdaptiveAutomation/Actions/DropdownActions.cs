using System;
using System.Linq;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Utilities;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AdaptiveAutomation.Actions;

public class DropdownActions
{
    private readonly IWebDriver _driver;
    private readonly ILogger? _logger;

    public DropdownActions(IWebDriver driver, ILogger? logger = null)
    {
        _driver = driver;
        _logger = logger;
    }

    public string SelectOption(ElementCandidate candidate, string targetOption)
    {
        // 1. Eligibility validation
        if (!candidate.IsEnabled || !candidate.Element.Enabled)
        {
            throw new InvalidElementStateException($"Dropdown candidate '{candidate}' is disabled and cannot be selected.");
        }

        if (!candidate.IsVisible || !candidate.Element.Displayed)
        {
            throw new ElementNotInteractableException($"Dropdown candidate '{candidate}' is hidden and cannot be interacted with.");
        }

        var selectElement = new SelectElement(candidate.Element);
        var options = selectElement.Options;

        if (options.Count == 0)
        {
            throw new InvalidOperationException($"Dropdown {candidate} contains no <option> elements.");
        }

        IWebElement? matchingOption = null;

        // 1. Search for exact normalized text match
        foreach (var opt in options)
        {
            if (TextNormalizer.ExactMatch(opt.Text, targetOption) ||
                TextNormalizer.ExactMatch(opt.GetAttribute("value"), targetOption))
            {
                matchingOption = opt;
                break;
            }
        }

        // 2. Search for whole-word boundary match (only if exact match not found; prevents "Indiana" matching "India")
        if (matchingOption == null)
        {
            foreach (var opt in options)
            {
                if (TextNormalizer.MatchesWord(opt.Text, targetOption) ||
                    TextNormalizer.MatchesWord(opt.GetAttribute("value"), targetOption))
                {
                    matchingOption = opt;
                    break;
                }
            }
        }

        if (matchingOption == null)
        {
            string available = string.Join(", ", options.Select(o => o.Text.Trim()));
            throw new NoSuchElementException($"Option '{targetOption}' not found in dropdown {candidate}. Available options: [{available}]");
        }

        WaitHelper.Retry(() =>
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", candidate.Element);
            selectElement.SelectByText(matchingOption.Text);
        });

        // Verification of selected option
        string selectedText = selectElement.SelectedOption.Text.Trim();
        string selectedValue = selectElement.SelectedOption.GetAttribute("value") ?? string.Empty;

        _logger?.LogInformation("Dropdown option selected. Text='{Text}', Value='{Value}'", selectedText, selectedValue);
        return selectedText;
    }
}

