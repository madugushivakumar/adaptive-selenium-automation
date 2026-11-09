using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using NUnit.Framework;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class StateTests : BaseTest
{
    [Test]
    public void State_ShouldBeDynamicallyIdentified_AndOptionSelected()
    {
        string url = GetFixtureUrl("baseline.html");
        BrowserManager.NavigateTo(url);

        var fieldDef = FieldDefinition.CreateState(Settings.TestData.State);
        var matchResult = ElementFinder.FindField(fieldDef);

        Assert.That(matchResult.IsSuccessfulMatch, Is.True,
            $"Expected successful match for State dropdown. Reason: {matchResult.DiagnosticExplanation}");
        Assert.That(matchResult.Candidate, Is.Not.Null);
        Assert.That(matchResult.Score, Is.GreaterThanOrEqualTo(Settings.Matching.MinimumConfidenceScore));

        var actions = new DropdownActions(Driver);
        string selectedOption = actions.SelectOption(matchResult.Candidate!, Settings.TestData.State);

        Assert.That(selectedOption, Does.Contain(Settings.TestData.State).IgnoreCase,
            $"Selected dropdown option '{selectedOption}' did not match expected '{Settings.TestData.State}'.");
    }
}

