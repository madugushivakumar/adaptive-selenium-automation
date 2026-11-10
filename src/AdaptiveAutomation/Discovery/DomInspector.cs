using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveAutomation.Models;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Discovery;

public class DomInspector
{
    private readonly IWebDriver _driver;

    public DomInspector(IWebDriver driver)
    {
        _driver = driver;
    }

    public IReadOnlyList<ElementCandidate> InspectCandidates(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.TextInput => InspectTextInputCandidates(),
            FieldType.RadioGroup => InspectRadioGroupCandidates(),
            FieldType.Dropdown => InspectDropdownCandidates(),
            _ => new List<ElementCandidate>()
        };
    }

    private IReadOnlyList<ElementCandidate> InspectTextInputCandidates()
    {
        var candidates = new List<ElementCandidate>();
        var inputs = _driver.FindElements(By.TagName("input"));

        var excludedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hidden", "submit", "button", "reset", "image", "file", "radio", "checkbox", "password",
            "email", "number", "tel", "range", "color", "date", "datetime-local", "month", "time", "week"
        };

        foreach (var input in inputs)
        {
            try
            {
                string type = input.GetAttribute("type") ?? "text";
                if (excludedTypes.Contains(type))
                {
                    continue;
                }

                bool isVisible = false;
                bool isEnabled = false;
                try
                {
                    isVisible = input.Displayed;
                    isEnabled = input.Enabled;
                }
                catch { }

                string id = input.GetAttribute("id") ?? string.Empty;
                string name = input.GetAttribute("name") ?? string.Empty;
                string placeholder = input.GetAttribute("placeholder") ?? string.Empty;
                string ariaLabel = input.GetAttribute("aria-label") ?? string.Empty;
                string title = input.GetAttribute("title") ?? string.Empty;
                string val = input.GetAttribute("value") ?? string.Empty;

                string associatedLabel = ContextAnalyzer.FindAssociatedLabel(_driver, input);
                string enclosingLabel = ContextAnalyzer.FindEnclosingLabel(input);
                string ariaLabelledBy = ContextAnalyzer.FindAriaLabelledBy(_driver, input);
                string nearbyContext = ContextAnalyzer.FindNearbyContext(input);

                var candidate = new ElementCandidate
                {
                    Element = input,
                    TagName = "input",
                    InputType = type,
                    Id = id,
                    Name = name,
                    Placeholder = placeholder,
                    AriaLabel = ariaLabel,
                    AriaLabelledByText = ariaLabelledBy,
                    Title = title,
                    CurrentValue = val,
                    AssociatedLabelText = associatedLabel,
                    EnclosingLabelText = enclosingLabel,
                    NearbyContextText = nearbyContext,
                    IsVisible = isVisible,
                    IsEnabled = isEnabled
                };

                candidates.Add(candidate);
            }
            catch (StaleElementReferenceException)
            {
                // Element was modified or removed dynamically; skip
            }
        }

        return candidates;
    }

    private IReadOnlyList<ElementCandidate> InspectRadioGroupCandidates()
    {
        var candidates = new List<ElementCandidate>();
        var radios = _driver.FindElements(By.CssSelector("input[type='radio']"));

        if (radios.Count == 0)
        {
            return candidates;
        }

        // Group radios: first by 'name' attribute if non-empty, otherwise by common parent container
        var groupedByAttr = new Dictionary<string, List<IWebElement>>();

        foreach (var radio in radios)
        {
            try
            {
                if (!radio.Displayed) continue;

                string name = radio.GetAttribute("name") ?? string.Empty;
                string groupKey;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    groupKey = "name:" + name;
                }
                else
                {
                    // Find nearest ancestor container above label: fieldset, role='radiogroup', form-group, row, etc.
                    try
                    {
                        var container = radio.FindElement(By.XPath("./ancestor::*[self::fieldset or @role='radiogroup' or contains(@class, 'group') or contains(@class, 'row') or contains(@class, 'flex') or contains(@class, 'block') or self::form][1]"));
                        groupKey = "container:" + container.GetHashCode();
                    }
                    catch
                    {
                        var parent = radio.FindElement(By.XPath("./parent::*"));
                        groupKey = "parent:" + parent.GetHashCode();
                    }
                }

                if (!groupedByAttr.TryGetValue(groupKey, out var list))
                {
                    list = new List<IWebElement>();
                    groupedByAttr[groupKey] = list;
                }
                list.Add(radio);
            }
            catch (StaleElementReferenceException) { }
        }

        foreach (var (key, groupRadios) in groupedByAttr)
        {
            if (groupRadios.Count == 0) continue;

            var firstRadio = groupRadios[0];
            string groupName = firstRadio.GetAttribute("name") ?? string.Empty;

            // Harvest group labels, container text, or fieldset legend
            string nearbyContext = ContextAnalyzer.FindNearbyContext(firstRadio);
            string associatedLabel = ContextAnalyzer.FindAssociatedLabel(_driver, firstRadio);

            var options = new List<string>();
            var optionMap = new Dictionary<string, IWebElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var radio in groupRadios)
            {
                string val = radio.GetAttribute("value") ?? string.Empty;
                string optText = val;

                // 1. Check if enclosed in a label
                try
                {
                    var enclosingLabel = radio.FindElements(By.XPath("./ancestor::label[1]"));
                    if (enclosingLabel.Count > 0 && !string.IsNullOrWhiteSpace(enclosingLabel[0].Text))
                    {
                        optText = enclosingLabel[0].Text.Trim();
                    }
                }
                catch { }

                // 2. Check adjacent sibling label or span
                if (string.IsNullOrWhiteSpace(optText) || optText == val)
                {
                    try
                    {
                        var adjacent = radio.FindElements(By.XPath("./following-sibling::*[self::label or self::span][1]"));
                        if (adjacent.Count > 0 && !string.IsNullOrWhiteSpace(adjacent[0].Text))
                        {
                            optText = adjacent[0].Text.Trim();
                        }
                    }
                    catch { }
                }

                if (!string.IsNullOrWhiteSpace(optText))
                {
                    options.Add(optText);
                    optionMap[optText] = radio;
                }
                if (!string.IsNullOrWhiteSpace(val) && !optionMap.ContainsKey(val))
                {
                    optionMap[val] = radio;
                }
            }

            // Extract group label from container if not found via direct label, or if direct label is just an option label
            if (string.IsNullOrWhiteSpace(associatedLabel) || options.Contains(associatedLabel, StringComparer.OrdinalIgnoreCase))
            {
                associatedLabel = string.Empty;

                // 1. Check closest fieldset legend first
                try
                {
                    var fieldsets = firstRadio.FindElements(By.XPath("./ancestor::fieldset[1]"));
                    if (fieldsets.Count > 0)
                    {
                        var legends = fieldsets[0].FindElements(By.XPath(".//legend"));
                        if (legends.Count > 0)
                        {
                            string legText = legends[0].Text.Trim();
                            if (!string.IsNullOrWhiteSpace(legText) && !options.Contains(legText, StringComparer.OrdinalIgnoreCase))
                            {
                                associatedLabel = legText;
                            }
                        }
                    }
                }
                catch { }

                // 2. Check nearest ancestor container headings (closest row/group/field/radiogroup)
                if (string.IsNullOrWhiteSpace(associatedLabel))
                {
                    try
                    {
                        var nearestContainers = firstRadio.FindElements(By.XPath("./ancestor::*[contains(@class, 'group') or contains(@class, 'row') or contains(@class, 'field') or @role='radiogroup'][1]"));
                        if (nearestContainers.Count > 0)
                        {
                            var headings = nearestContainers[0].FindElements(By.XPath(".//legend | .//label[not(ancestor::label)] | .//*[contains(@class, 'label') or contains(@class, 'caption') or contains(@class, 'heading') or contains(@class, 'header') or contains(@class, 'title') or contains(@class, 'sec-title')]"));
                            foreach (var h in headings)
                            {
                                string hText = h.Text.Trim();
                                if (!string.IsNullOrWhiteSpace(hText) && !options.Contains(hText, StringComparer.OrdinalIgnoreCase))
                                {
                                    associatedLabel = hText;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                }

                // 3. Fallback to immediately preceding label, legend, or heading
                if (string.IsNullOrWhiteSpace(associatedLabel))
                {
                    try
                    {
                        var preceding = firstRadio.FindElements(By.XPath("./preceding::*[self::label or self::legend or contains(@class, 'label') or contains(@class, 'caption') or contains(@class, 'heading') or contains(@class, 'header') or contains(@class, 'title')][1]"));
                        if (preceding.Count > 0)
                        {
                            string pText = preceding[0].Text.Trim();
                            if (!string.IsNullOrWhiteSpace(pText) && !options.Contains(pText, StringComparer.OrdinalIgnoreCase))
                            {
                                associatedLabel = pText;
                            }
                        }
                    }
                    catch { }
                }
            }

            var groupCandidate = new ElementCandidate
            {
                Element = firstRadio,
                TagName = "input",
                InputType = "radio-group",
                Name = groupName,
                AssociatedLabelText = associatedLabel,
                NearbyContextText = nearbyContext,
                AvailableOptions = options,
                GroupElements = groupRadios,
                OptionElementMap = optionMap,
                IsVisible = true,
                IsEnabled = true
            };

            candidates.Add(groupCandidate);
        }

        return candidates;
    }

    private IReadOnlyList<ElementCandidate> InspectDropdownCandidates()
    {
        var candidates = new List<ElementCandidate>();
        var selects = _driver.FindElements(By.TagName("select"));

        foreach (var select in selects)
        {
            try
            {
                bool isVisible = false;
                bool isEnabled = false;
                try
                {
                    isVisible = select.Displayed;
                    isEnabled = select.Enabled;
                }
                catch { }

                string id = select.GetAttribute("id") ?? string.Empty;
                string name = select.GetAttribute("name") ?? string.Empty;
                string ariaLabel = select.GetAttribute("aria-label") ?? string.Empty;
                string title = select.GetAttribute("title") ?? string.Empty;

                string associatedLabel = ContextAnalyzer.FindAssociatedLabel(_driver, select);
                string enclosingLabel = ContextAnalyzer.FindEnclosingLabel(select);
                string ariaLabelledBy = ContextAnalyzer.FindAriaLabelledBy(_driver, select);
                string nearbyContext = ContextAnalyzer.FindNearbyContext(select);

                var options = new List<string>();
                try
                {
                    var optionElements = select.FindElements(By.TagName("option"));
                    foreach (var opt in optionElements)
                    {
                        string text = opt.Text?.Trim() ?? string.Empty;
                        if (!string.IsNullOrEmpty(text))
                        {
                            options.Add(text);
                        }
                    }
                }
                catch { }

                var candidate = new ElementCandidate
                {
                    Element = select,
                    TagName = "select",
                    InputType = "select",
                    Id = id,
                    Name = name,
                    AriaLabel = ariaLabel,
                    AriaLabelledByText = ariaLabelledBy,
                    Title = title,
                    AssociatedLabelText = associatedLabel,
                    EnclosingLabelText = enclosingLabel,
                    NearbyContextText = nearbyContext,
                    AvailableOptions = options,
                    IsVisible = isVisible,
                    IsEnabled = isEnabled
                };

                candidates.Add(candidate);
            }
            catch (StaleElementReferenceException) { }
        }

        return candidates;
    }
}

