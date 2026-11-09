using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using NUnit.Framework;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class FirstNameTests : BaseTest
{
    [Test]
    public void FirstName_ShouldBeDynamicallyIdentified_AndFilledCorrectly()
    {
        string url = GetFixtureUrl("baseline.html");
        BrowserManager.NavigateTo(url);

        var fieldDef = FieldDefinition.CreateFirstName(Settings.TestData.FirstName);
        var matchResult = ElementFinder.FindField(fieldDef);

        Assert.That(matchResult.IsSuccessfulMatch, Is.True,
            $"Expected successful match for First Name. Reason: {matchResult.DiagnosticExplanation}");
        Assert.That(matchResult.Candidate, Is.Not.Null);
        Assert.That(matchResult.Score, Is.GreaterThanOrEqualTo(Settings.Matching.MinimumConfidenceScore));

        var actions = new TextFieldActions(Driver);
        string actualValue = actions.SetText(matchResult.Candidate!, Settings.TestData.FirstName);

        Assert.That(actualValue, Is.EqualTo(Settings.TestData.FirstName),
            $"Entered value '{actualValue}' did not match expected '{Settings.TestData.FirstName}'.");
    }
}

