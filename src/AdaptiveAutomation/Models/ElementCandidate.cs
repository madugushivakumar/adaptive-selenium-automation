using System.Collections.Generic;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Models;

public class ElementCandidate
{
    public required IWebElement Element { get; init; }
    public string TagName { get; init; } = string.Empty;
    public string InputType { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Placeholder { get; init; } = string.Empty;
    public string AssociatedLabelText { get; init; } = string.Empty;
    public string EnclosingLabelText { get; init; } = string.Empty;
    public string NearbyContextText { get; init; } = string.Empty;
    public string AriaLabel { get; init; } = string.Empty;
    public string AriaLabelledByText { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string CurrentValue { get; init; } = string.Empty;
    public IReadOnlyList<string> AvailableOptions { get; init; } = new List<string>();
    public bool IsVisible { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsRequired { get; init; }

    // Associated sub-elements, e.g., radio group members
    public IReadOnlyList<IWebElement> GroupElements { get; init; } = new List<IWebElement>();
    public IReadOnlyDictionary<string, IWebElement> OptionElementMap { get; init; } = new Dictionary<string, IWebElement>();

    // Audit trail of score calculation
    public Dictionary<string, double> MatchEvidence { get; } = new();

    public override string ToString()
    {
        return $"<{TagName} id='{Id}' name='{Name}' type='{InputType}' label='{AssociatedLabelText}' placeholder='{Placeholder}'>";
    }
}

