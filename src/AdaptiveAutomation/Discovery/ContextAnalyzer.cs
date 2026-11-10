using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AdaptiveAutomation.Utilities;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Discovery;

public static class ContextAnalyzer
{
    public static string FindAssociatedLabel(IWebDriver driver, IWebElement element)
    {
        try
        {
            string id = element.GetAttribute("id") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var labels = driver.FindElements(By.CssSelector($"label[for='{id}']"));
                foreach (var label in labels)
                {
                    string text = label.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }
            }
        }
        catch
        {
            // Ignore stale or detached references during lookup
        }

        return string.Empty;
    }

    public static string FindEnclosingLabel(IWebElement element)
    {
        try
        {
            var parent = element.FindElement(By.XPath("./ancestor::label[1]"));
            if (parent != null)
            {
                return parent.Text.Trim();
            }
        }
        catch
        {
            // Not enclosed in a label tag
        }

        return string.Empty;
    }

    public static string FindAriaLabelledBy(IWebDriver driver, IWebElement element)
    {
        try
        {
            string labelledBy = element.GetAttribute("aria-labelledby") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(labelledBy))
            {
                var ids = labelledBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var texts = new List<string>();
                foreach (var id in ids)
                {
                    try
                    {
                        var el = driver.FindElement(By.Id(id));
                        if (!string.IsNullOrWhiteSpace(el.Text))
                        {
                            texts.Add(el.Text.Trim());
                        }
                    }
                    catch
                    {
                        // Ignore missing target ID
                    }
                }

                if (texts.Count > 0)
                {
                    return string.Join(" ", texts);
                }
            }
        }
        catch
        {
            // Ignore
        }

        return string.Empty;
    }

    public static string FindNearbyContext(IWebElement element)
    {
        var contextPieces = new List<string>();

        try
        {
            // 1. Look for preceding label or span sibling, or enclosing label
            try
            {
                var enclosingLabels = element.FindElements(By.XPath("./ancestor::label[1]"));
                if (enclosingLabels.Count > 0 && !string.IsNullOrWhiteSpace(enclosingLabels[0].Text))
                {
                    string elText = enclosingLabels[0].Text.Trim();
                    if (!contextPieces.Contains(elText))
                    {
                        contextPieces.Add(elText);
                    }
                }

                var precedingSiblings = element.FindElements(By.XPath("./preceding-sibling::*[self::label or self::span or self::p or self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6][1]"));
                foreach (var s in precedingSiblings)
                {
                    if (!string.IsNullOrWhiteSpace(s.Text) && !contextPieces.Contains(s.Text.Trim()))
                    {
                        contextPieces.Add(s.Text.Trim());
                    }
                }
            }
            catch { }

            // 2. Look for container-level headings, legends, or enclosing label text
            try
            {
                IWebElement? container = null;
                var semanticContainers = element.FindElements(By.XPath("./ancestor::*[contains(@class, 'form-group') or contains(@class, 'form-row') or contains(@class, 'row') or contains(@class, 'field') or contains(@class, 'card') or contains(@class, 'section') or self::fieldset or self::form][1]"));
                if (semanticContainers.Count > 0)
                {
                    container = semanticContainers[0];
                }
                else
                {
                    var divContainers = element.FindElements(By.XPath("./ancestor::div[1]"));
                    if (divContainers.Count > 0) container = divContainers[0];
                }

                if (container != null)
                {
                    // Find legends, labels, or heading elements inside container
                    var headings = container.FindElements(By.XPath(".//legend | .//label | .//span[contains(@class, 'label') or contains(@class, 'title') or contains(@class, 'heading') or contains(@class, 'caption')] | .//div[contains(@class, 'heading') or contains(@class, 'label')]"));
                    foreach (var h in headings)
                    {
                        if (!string.IsNullOrWhiteSpace(h.Text) && !contextPieces.Contains(h.Text.Trim()))
                        {
                            contextPieces.Add(h.Text.Trim());
                        }
                    }

                    // Also include direct text of container or enclosing label if present
                    if (headings.Count == 0 && !string.IsNullOrWhiteSpace(container.Text))
                    {
                        string cText = container.Text.Trim();
                        if (!contextPieces.Contains(cText))
                        {
                            contextPieces.Add(cText);
                        }
                    }
                }
            }
            catch { }
        }
        catch
        {
            // Ignore traversal issues
        }

        return string.Join(" | ", contextPieces);
    }
}

