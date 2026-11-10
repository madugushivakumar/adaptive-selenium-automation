using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using NUnit.Framework;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class SimilarFieldsExpandedTests : BaseTest
{
    [Test]
    public void Mutation4_SimilarFields_Expanded_ShouldDisambiguateAllTargetsAccurately()
    {
        string url = GetFixtureUrl("similar-fields-expanded.html");
        BrowserManager.NavigateTo(url);

        // 1. First Name Identification
        // The page contains: First Name, Middle Name, Company Name, Last Name, Email, Phone
        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);

        Assert.That(fnResult.IsSuccessfulMatch, Is.True,
            $"Expected First Name match. Reason: {fnResult.DiagnosticExplanation}");
        Assert.That(fnResult.Candidate!.Id, Is.EqualTo("fld_target_fname"),
            "Expected engine to select First Name ('fld_target_fname'), NOT Middle Name ('fld_mid_init') or Company Name ('fld_org_legal') or Last Name ('fld_sur_name').");

        var textActions = new TextFieldActions(Driver);
        string enteredName = textActions.SetText(fnResult.Candidate, "Shiva");
        Assert.That(enteredName, Is.EqualTo("Shiva"));

        // 2. Gender Group Identification
        // The page contains: Subscription Preference, Gender, Employment Status
        var genDef = FieldDefinition.CreateGender("Transgender");
        var genResult = ElementFinder.FindField(genDef);

        Assert.That(genResult.IsSuccessfulMatch, Is.True,
            $"Expected Gender match. Reason: {genResult.DiagnosticExplanation}");
        Assert.That(genResult.Candidate!.Name, Is.EqualTo("ident_gender_opt"),
            "Expected engine to select Gender radio group ('ident_gender_opt'), NOT Subscription Preference ('pref_cadence') or Employment Status ('work_status').");

        var radioActions = new RadioGroupActions(Driver);
        string selectedGender = radioActions.SelectOption(genResult.Candidate, "Transgender");
        Assert.That(selectedGender, Does.Contain("Transgender").IgnoreCase);

        // 3. State Dropdown Identification
        // The page contains: Country, City, Department, State
        var stateDef = FieldDefinition.CreateState("Telangana");
        var stateResult = ElementFinder.FindField(stateDef);

        Assert.That(stateResult.IsSuccessfulMatch, Is.True,
            $"Expected State match. Reason: {stateResult.DiagnosticExplanation}");
        Assert.That(stateResult.Candidate!.Id, Is.EqualTo("cbo_target_state"),
            "Expected engine to select State dropdown ('cbo_target_state'), NOT Country ('cbo_nation'), City ('cbo_metro_city'), or Department ('cbo_dept_unit').");

        var dropdownActions = new DropdownActions(Driver);
        string selectedState = dropdownActions.SelectOption(stateResult.Candidate, "Telangana");
        Assert.That(selectedState, Does.Contain("Telangana").IgnoreCase);
    }
}

