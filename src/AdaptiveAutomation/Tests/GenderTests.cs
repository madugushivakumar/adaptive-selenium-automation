using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using NUnit.Framework;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class GenderTests : BaseTest
{
    [Test]
    public void Gender_ShouldBeDynamicallyIdentified_AndOptionSelected()
    {
        string url = GetFixtureUrl("baseline.html");
        BrowserManager.NavigateTo(url);

        var fieldDef = FieldDefinition.CreateGender(Settings.TestData.Gender);
        var matchResult = ElementFinder.FindField(fieldDef);

        Assert.That(matchResult.IsSuccessfulMatch, Is.True,
            $"Expected successful match for Gender. Reason: {matchResult.DiagnosticExplanation}");
        Assert.That(matchResult.Candidate, Is.Not.Null);
        Assert.That(matchResult.Score, Is.GreaterThanOrEqualTo(Settings.Matching.MinimumConfidenceScore));

        var actions = new RadioGroupActions(Driver);
        string selectedOption = actions.SelectOption(matchResult.Candidate!, Settings.TestData.Gender);

        Assert.That(selectedOption, Does.Contain(Settings.TestData.Gender).IgnoreCase,
            $"Selected radio '{selectedOption}' did not match expected '{Settings.TestData.Gender}'.");
    }
}

