using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Utilities;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Actions;

public class RadioGroupActions
{
    private readonly IWebDriver _driver;
    private readonly ILogger? _logger;

    public RadioGroupActions(IWebDriver driver, ILogger? logger = null)
    {
        _driver = driver;
        _logger = logger;
    }

    public string SelectOption(ElementCandidate candidate, string targetOption)
    {
        if (candidate.GroupElements == null || candidate.GroupElements.Count == 0)
        {
            throw new InvalidOperationException($"No radio options found in candidate group: {candidate}");
        }

        IWebElement? matchingRadio = null;
        string selectedOptionLabel = string.Empty;

        // 1. Try dictionary lookup
        foreach (var (optText, radio) in candidate.OptionElementMap)
        {
            if (TextNormalizer.ExactMatch(optText, targetOption) ||
                TextNormalizer.ContainsNormalized(optText, targetOption))
            {
                matchingRadio = radio;
                selectedOptionLabel = optText;
                break;
            }
        }

        // 2. Fallback to iterating elements and checking value or adjacent label
        if (matchingRadio == null)
        {
            foreach (var radio in candidate.GroupElements)
            {
                string val = radio.GetAttribute("value") ?? string.Empty;
                if (TextNormalizer.ExactMatch(val, targetOption))
                {
                    matchingRadio = radio;
                    selectedOptionLabel = val;
                    break;
                }
            }
        }

        if (matchingRadio == null)
        {
            string available = string.Join(", ", candidate.AvailableOptions);
            throw new NoSuchElementException($"Option '{targetOption}' not found in radio group. Available options: [{available}]");
        }

        // Check if option is disabled
        if (!matchingRadio.Enabled)
        {
            throw new InvalidElementStateException($"Radio option '{targetOption}' in group '{candidate}' is disabled and cannot be selected.");
        }

        if (!matchingRadio.Displayed)
        {
            throw new ElementNotInteractableException($"Radio option '{targetOption}' in group '{candidate}' is hidden and cannot be interacted with.");
        }

        WaitHelper.Retry(() =>
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", matchingRadio);

            try
            {
                matchingRadio.Click();
            }
            catch (ElementClickInterceptedException)
            {
                // Fallback click via JavaScript for customized or overlayed radio controls
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", matchingRadio);
            }
        });

        // Verification of selection
        bool isSelected = matchingRadio.Selected;
        if (!isSelected)
        {
            // Try JS click once more if standard click didn't trigger
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", matchingRadio);
            isSelected = matchingRadio.Selected;
        }

        if (!isSelected)
        {
            throw new InvalidOperationException($"Failed to select radio option '{targetOption}' in group '{candidate}'. Selected state remained false.");
        }

        _logger?.LogInformation("Radio option '{Option}' selected: SelectedState={State}", selectedOptionLabel, isSelected);
        return selectedOptionLabel;
    }
}

