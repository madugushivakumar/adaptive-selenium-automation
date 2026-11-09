using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using NUnit.Framework;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class AdaptiveRegressionTests : BaseTest
{
    [Test]
    public void Mutation1_ChangedAttributes_ShouldIdentifyAndInteractSuccessfully()
    {
        string url = GetFixtureUrl("changed-attributes.html");
        BrowserManager.NavigateTo(url);

        // 1. First Name
        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);
        Assert.That(fnResult.IsSuccessfulMatch, Is.True, fnResult.DiagnosticExplanation);
        var textActions = new TextFieldActions(Driver);
        Assert.That(textActions.SetText(fnResult.Candidate!, "Shiva"), Is.EqualTo("Shiva"));

        // 2. Gender
        var genDef = FieldDefinition.CreateGender("Male");
        var genResult = ElementFinder.FindField(genDef);
        Assert.That(genResult.IsSuccessfulMatch, Is.True, genResult.DiagnosticExplanation);
        var radioActions = new RadioGroupActions(Driver);
        Assert.That(radioActions.SelectOption(genResult.Candidate!, "Male"), Does.Contain("Male").IgnoreCase);

        // 3. State
        var stateDef = FieldDefinition.CreateState("India");
        var stateResult = ElementFinder.FindField(stateDef);
        Assert.That(stateResult.IsSuccessfulMatch, Is.True, stateResult.DiagnosticExplanation);
        var dropdownActions = new DropdownActions(Driver);
        Assert.That(dropdownActions.SelectOption(stateResult.Candidate!, "India"), Does.Contain("India").IgnoreCase);
    }

    [Test]
    public void Mutation2_ReorderedFields_ShouldIdentifyFieldsRegardlessOfOrder()
    {
        string url = GetFixtureUrl("reordered-fields.html");
        BrowserManager.NavigateTo(url);

        // In this fixture, State is first, Gender is second, Last Name is third, First Name is last
        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);
        Assert.That(fnResult.IsSuccessfulMatch, Is.True);
        new TextFieldActions(Driver).SetText(fnResult.Candidate!, "Shiva");

        var genDef = FieldDefinition.CreateGender("Female");
        var genResult = ElementFinder.FindField(genDef);
        Assert.That(genResult.IsSuccessfulMatch, Is.True, genResult.DiagnosticExplanation);
        new RadioGroupActions(Driver).SelectOption(genResult.Candidate!, "Female");

        var stateDef = FieldDefinition.CreateState("India");
        var stateResult = ElementFinder.FindField(stateDef);
        Assert.That(stateResult.IsSuccessfulMatch, Is.True);
        new DropdownActions(Driver).SelectOption(stateResult.Candidate!, "India");
    }

    [Test]
    public void Mutation3_ChangedStructure_ShouldIdentifyWithNestedContainersAndAria()
    {
        string url = GetFixtureUrl("changed-structure.html");
        BrowserManager.NavigateTo(url);

        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);
        Assert.That(fnResult.IsSuccessfulMatch, Is.True);
        new TextFieldActions(Driver).SetText(fnResult.Candidate!, "Shiva");

        var genDef = FieldDefinition.CreateGender("Transgender");
        var genResult = ElementFinder.FindField(genDef);
        Assert.That(genResult.IsSuccessfulMatch, Is.True, genResult.DiagnosticExplanation);
        new RadioGroupActions(Driver).SelectOption(genResult.Candidate!, "Transgender");

        var stateDef = FieldDefinition.CreateState("India");
        var stateResult = ElementFinder.FindField(stateDef);
        Assert.That(stateResult.IsSuccessfulMatch, Is.True);
        new DropdownActions(Driver).SelectOption(stateResult.Candidate!, "India");
    }

    [Test]
    public void Mutation4_SimilarFields_ShouldSelectTargetFieldAndAvoidConfusingCompetitors()
    {
        string url = GetFixtureUrl("similar-fields.html");
        BrowserManager.NavigateTo(url);

        // Page has Company Name, Middle Name, First Name, Last Name.
        // It must select First Name, NOT Last Name or Middle Name.
        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);
        Assert.That(fnResult.IsSuccessfulMatch, Is.True);
        Assert.That(fnResult.Candidate!.Id, Is.EqualTo("first_name"));

        // Page has Marital Status, Gender, Communication Preference radio groups.
        // It must select Gender.
        var genDef = FieldDefinition.CreateGender("Male");
        var genResult = ElementFinder.FindField(genDef);
        Assert.That(genResult.IsSuccessfulMatch, Is.True);
        Assert.That(genResult.Candidate!.Name, Is.EqualTo("gender"));

        // Page has Country, State, City selects.
        // It must select State.
        var stateDef = FieldDefinition.CreateState("India");
        var stateResult = ElementFinder.FindField(stateDef);
        Assert.That(stateResult.IsSuccessfulMatch, Is.True);
        Assert.That(stateResult.Candidate!.Id, Is.EqualTo("state_select"));
    }

    [Test]
    public void Mutation6_AmbiguousFields_ShouldDetectAmbiguityAndRefuseArbitraryInteraction()
    {
        string url = GetFixtureUrl("ambiguous-fields.html");
        BrowserManager.NavigateTo(url);

        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);

        Assert.That(fnResult.IsAmbiguous, Is.True, "Expected engine to report ambiguity when 2 identical fields exist.");
        Assert.That(fnResult.IsSuccessfulMatch, Is.False);
        Assert.That(fnResult.DiagnosticExplanation, Does.Contain("Ambiguous match detected"));
    }

    [Test]
    public void Mutation7_MissingFields_ShouldReportClearDiagnosticFailure()
    {
        string url = GetFixtureUrl("missing-fields.html");
        BrowserManager.NavigateTo(url);

        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);

        Assert.That(fnResult.IsSuccessfulMatch, Is.False);
        Assert.That(fnResult.Candidate == null || fnResult.Score < Settings.Matching.MinimumConfidenceScore, Is.True);
    }
}

