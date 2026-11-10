using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using NUnit.Framework;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class LabelVariationTests : BaseTest
{
    [Test]
    [TestCase("label-variations.html", "Given Name", "user_given_name", "Shiva")]
    [TestCase("label-variations-2.html", "Your First Name", "txt_your_first_name", "Shiva")]
    [TestCase("label-variations-3.html", "Enter First Name", "entry_fname", "Shiva")]
    public void Mutation5_LabelVariations_FirstName_ShouldIdentifyAllSynonyms(string fixtureFile, string synonymLabel, string expectedName, string valueToEnter)
    {
        string url = GetFixtureUrl(fixtureFile);
        BrowserManager.NavigateTo(url);

        var fieldDef = new FieldDefinition
        {
            FieldName = "First Name",
            FieldType = FieldType.TextInput,
            Synonyms = new[] { synonymLabel },
            NegativeKeywords = new[] { "Last Name", "Company", "Middle" },
            ExpectedValue = valueToEnter
        };

        var matchResult = ElementFinder.FindField(fieldDef);

        Assert.That(matchResult.IsSuccessfulMatch, Is.True,
            $"Expected successful match for variation '{synonymLabel}'. Reason: {matchResult.DiagnosticExplanation}");
        Assert.That(matchResult.Candidate!.Name, Is.EqualTo(expectedName));

        var actions = new TextFieldActions(Driver);
        string actual = actions.SetText(matchResult.Candidate, valueToEnter);
        Assert.That(actual, Is.EqualTo(valueToEnter));
    }

    [Test]
    [TestCase("label-variations.html", "Select Gender", "grp_sex", "Male")]
    [TestCase("label-variations-2.html", "Your Gender", "rad_gender_pref", "Female")]
    public void Mutation5_LabelVariations_Gender_ShouldIdentifyAllSynonyms(string fixtureFile, string synonymLabel, string expectedGroupName, string optionToSelect)
    {
        string url = GetFixtureUrl(fixtureFile);
        BrowserManager.NavigateTo(url);

        var fieldDef = new FieldDefinition
        {
            FieldName = "Gender",
            FieldType = FieldType.RadioGroup,
            Synonyms = new[] { synonymLabel },
            ExpectedValue = optionToSelect
        };

        var matchResult = ElementFinder.FindField(fieldDef);

        Assert.That(matchResult.IsSuccessfulMatch, Is.True,
            $"Expected successful match for variation '{synonymLabel}'. Reason: {matchResult.DiagnosticExplanation}");
        Assert.That(matchResult.Candidate!.Name, Is.EqualTo(expectedGroupName));

        var actions = new RadioGroupActions(Driver);
        string actual = actions.SelectOption(matchResult.Candidate, optionToSelect);
        Assert.That(actual, Does.Contain(optionToSelect).IgnoreCase);
    }

    [Test]
    [TestCase("label-variations.html", "State/Province", "geo_province", "India")]
    [TestCase("label-variations-2.html", "Choose State", "opt_chosen_state", "Texas")]
    [TestCase("label-variations-3.html", "Select State", "sel_target_state", "Ontario")]
    public void Mutation5_LabelVariations_State_ShouldIdentifyAllSynonyms(string fixtureFile, string synonymLabel, string expectedName, string optionToSelect)
    {
        string url = GetFixtureUrl(fixtureFile);
        BrowserManager.NavigateTo(url);

        var fieldDef = new FieldDefinition
        {
            FieldName = "State",
            FieldType = FieldType.Dropdown,
            Synonyms = new[] { synonymLabel },
            ExpectedValue = optionToSelect
        };

        var matchResult = ElementFinder.FindField(fieldDef);

        Assert.That(matchResult.IsSuccessfulMatch, Is.True,
            $"Expected successful match for variation '{synonymLabel}'. Reason: {matchResult.DiagnosticExplanation}");
        Assert.That(matchResult.Candidate!.Name, Is.EqualTo(expectedName));

        var actions = new DropdownActions(Driver);
        string actual = actions.SelectOption(matchResult.Candidate, optionToSelect);
        Assert.That(actual, Does.Contain(optionToSelect).IgnoreCase);
    }

    [Test]
    public void Mutation5_UnsupportedLabels_ShouldSafelyRejectUnrelatedFields()
    {
        string url = GetFixtureUrl("unsupported-labels.html");
        BrowserManager.NavigateTo(url);

        // 1. First Name against unrelated form (Favorite Book, Shoe Size)
        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef);
        Assert.That(fnResult.IsSuccessfulMatch, Is.False, "Expected First Name to be rejected on form with only book and shoe size.");

        // 2. Gender against unrelated radio group (Operating System: Windows, Linux, macOS)
        var genDef = FieldDefinition.CreateGender("Male");
        var genResult = ElementFinder.FindField(genDef);
        Assert.That(genResult.IsSuccessfulMatch, Is.False, "Expected Gender to be rejected on form with only OS choices.");

        // 3. State against unrelated dropdown (Timezone: UTC, PST, IST)
        var stateDef = FieldDefinition.CreateState("India");
        var stateResult = ElementFinder.FindField(stateDef);
        Assert.That(stateResult.IsSuccessfulMatch, Is.False, "Expected State to be rejected on form with only Timezone dropdown.");
    }
}

