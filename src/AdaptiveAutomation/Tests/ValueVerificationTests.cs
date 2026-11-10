using System;
using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class ValueVerificationTests : BaseTest
{
    [Test]
    public void GenderSelection_WhenReordered_MustSelectMaleAndNotFemale()
    {
        string url = GetFixtureUrl("reordered-fields.html");
        BrowserManager.NavigateTo(url);

        var genDef = FieldDefinition.CreateGender("Male");
        var match = ElementFinder.FindField(genDef);
        Assert.That(match.IsSuccessfulMatch, Is.True, match.DiagnosticExplanation);

        var actions = new RadioGroupActions(Driver);
        string actualSelected = actions.SelectOption(match.Candidate!, "Male");

        // 1. Verify returned label is strictly Male, NOT Female
        Assert.That(actualSelected, Is.EqualTo("Male"),
            "Expected 'Male' to be selected, but actual selected label was '{0}'", actualSelected);

        // 2. Query DOM radio buttons directly to verify actual selection state
        var maleRadio = Driver.FindElement(By.CssSelector("input[type='radio'][value='Male']"));
        var femaleRadio = Driver.FindElement(By.CssSelector("input[type='radio'][value='Female']"));

        Assert.That(maleRadio.Selected, Is.True, "DOM verification failed: Male radio button is NOT selected.");
        Assert.That(femaleRadio.Selected, Is.False, "False positive detected: Female radio button was incorrectly selected.");
    }

    [Test]
    public void GenderSelection_WhenSelectingFemale_MustSelectFemaleAndNotMale()
    {
        string url = GetFixtureUrl("reordered-fields.html");
        BrowserManager.NavigateTo(url);

        var genDef = FieldDefinition.CreateGender("Female");
        var match = ElementFinder.FindField(genDef);
        Assert.That(match.IsSuccessfulMatch, Is.True);

        var actions = new RadioGroupActions(Driver);
        string actualSelected = actions.SelectOption(match.Candidate!, "Female");

        Assert.That(actualSelected, Is.EqualTo("Female"));

        var maleRadio = Driver.FindElement(By.CssSelector("input[type='radio'][value='Male']"));
        var femaleRadio = Driver.FindElement(By.CssSelector("input[type='radio'][value='Female']"));

        Assert.That(femaleRadio.Selected, Is.True);
        Assert.That(maleRadio.Selected, Is.False);
    }

    [Test]
    [TestCase("Female", "Male", false)]
    [TestCase("Male", "Female", false)]
    [TestCase("Transgender", "Male", false)]
    [TestCase("Male", "Male", true)]
    [TestCase("Female", "Female", true)]
    [TestCase("Male (Option 1)", "Male", true)]
    [TestCase("Female (Option 1)", "Male", false)]
    public void ValueVerification_GenderValueMatching_MustPreventFalsePositiveSubstringMatch(string actual, string expected, bool shouldMatch)
    {
        bool isMatch = TextNormalizer.IsMatchForValue(actual, expected);
        Assert.That(isMatch, Is.EqualTo(shouldMatch),
            $"IsMatchForValue('{actual}', '{expected}') evaluated to {isMatch}, expected {shouldMatch}");

        bool matchesWord = TextNormalizer.MatchesWord(actual, expected);
        Assert.That(matchesWord, Is.EqualTo(shouldMatch),
            $"MatchesWord('{actual}', '{expected}') evaluated to {matchesWord}, expected {shouldMatch}");
    }

    [Test]
    [TestCase("California", "India", false)]
    [TestCase("Indiana", "India", false)]
    [TestCase("India", "India", true)]
    [TestCase("India (IN)", "India", true)]
    [TestCase("Germany", "India", false)]
    public void ValueVerification_DropdownValueMatching_MustRejectMismatches(string actual, string expected, bool shouldMatch)
    {
        bool isMatch = TextNormalizer.IsMatchForValue(actual, expected);
        Assert.That(isMatch, Is.EqualTo(shouldMatch),
            $"IsMatchForValue('{actual}', '{expected}') evaluated to {isMatch}, expected {shouldMatch}");
    }

    [Test]
    [TestCase("WrongName", "Shiva", false)]
    [TestCase("Shiva", "Shiva", true)]
    [TestCase("Shiva Kumar", "Shiva", true)]
    [TestCase("Alexander", "Alex", false)]
    public void ValueVerification_TextInputValueMatching_MustRejectMismatches(string actual, string expected, bool shouldMatch)
    {
        bool isMatch = TextNormalizer.IsMatchForValue(actual, expected);
        Assert.That(isMatch, Is.EqualTo(shouldMatch),
            $"IsMatchForValue('{actual}', '{expected}') evaluated to {isMatch}, expected {shouldMatch}");
    }

    [Test]
    public void RadioGroupActions_WhenTargetOptionDoesNotExist_MustThrowNoSuchElementException()
    {
        string url = GetFixtureUrl("reordered-fields.html");
        BrowserManager.NavigateTo(url);

        var genDef = FieldDefinition.CreateGender("Male");
        var match = ElementFinder.FindField(genDef);
        Assert.That(match.IsSuccessfulMatch, Is.True);

        var actions = new RadioGroupActions(Driver);
        Assert.Throws<NoSuchElementException>(() => actions.SelectOption(match.Candidate!, "NonExistentGender"));
    }

    [Test]
    public void DropdownActions_WhenTargetOptionDoesNotExist_MustThrowNoSuchElementException()
    {
        string url = GetFixtureUrl("reordered-fields.html");
        BrowserManager.NavigateTo(url);

        var stateDef = FieldDefinition.CreateState("India");
        var match = ElementFinder.FindField(stateDef);
        Assert.That(match.IsSuccessfulMatch, Is.True);

        var actions = new DropdownActions(Driver);
        Assert.Throws<NoSuchElementException>(() => actions.SelectOption(match.Candidate!, "Atlantis"));
    }
}

