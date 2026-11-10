using System;
using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Models;
using NUnit.Framework;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Tests;

[TestFixture]
public class InteractionFailureTests : BaseTest
{
    [Test]
    public void Mutation8_ScenarioA_DisabledFirstName_ShouldDetectAndRefuseUnsafeInteraction()
    {
        string url = GetFixtureUrl("interaction-failures.html");
        BrowserManager.NavigateTo(url);

        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var match = ElementFinder.FindField(fnDef);

        Assert.That(match.Candidate, Is.Not.Null, "Candidate should be identified based on semantic label 'First Name'");
        Assert.That(match.Candidate!.IsEnabled, Is.False, "Candidate should be marked as disabled");

        var actions = new TextFieldActions(Driver);
        var ex = Assert.Throws<InvalidElementStateException>(() =>
        {
            actions.SetText(match.Candidate, "Shiva");
        });

        Assert.That(ex!.Message, Does.Contain("disabled"));
    }

    [Test]
    public void Mutation8_ScenarioB_HiddenFirstName_ShouldDetectAndRefuseInteraction()
    {
        string url = GetFixtureUrl("interaction-failures.html");
        BrowserManager.NavigateTo(url);

        // Target hidden input in form B
        var fnDef = new FieldDefinition
        {
            FieldName = "First Name",
            FieldType = FieldType.TextInput,
            Synonyms = new[] { "First Name" }
        };

        // Inject script to specifically locate the hidden one if needed or inspect
        var candidate = ElementFinder.FindField(fnDef);
        Assert.That(candidate.Candidate, Is.Not.Null);

        // Manually test a hidden candidate interaction failure
        var hiddenElement = Driver.FindElement(By.Id("hidden_fname"));
        var hiddenCandidate = new ElementCandidate
        {
            Element = hiddenElement,
            TagName = "input",
            InputType = "text",
            Id = "hidden_fname",
            IsVisible = false,
            IsEnabled = true
        };

        var actions = new TextFieldActions(Driver);
        var ex = Assert.Throws<ElementNotInteractableException>(() =>
        {
            actions.SetText(hiddenCandidate, "Shiva");
        });

        Assert.That(ex!.Message, Does.Contain("hidden"));
    }

    [Test]
    public void Mutation8_ScenarioC_DisabledGenderOption_ShouldDetectDisabledRadio()
    {
        string url = GetFixtureUrl("interaction-failures.html");
        BrowserManager.NavigateTo(url);

        var genDef = FieldDefinition.CreateGender("Female");
        var match = ElementFinder.FindField(genDef);

        Assert.That(match.IsSuccessfulMatch, Is.True);

        var actions = new RadioGroupActions(Driver);
        var ex = Assert.Throws<InvalidElementStateException>(() =>
        {
            actions.SelectOption(match.Candidate!, "Female");
        });

        Assert.That(ex!.Message, Does.Contain("disabled"));
    }

    [Test]
    public void Mutation8_ScenarioD_MissingStateOption_ShouldThrowDescriptiveNoSuchElement()
    {
        string url = GetFixtureUrl("interaction-failures.html");
        BrowserManager.NavigateTo(url);

        var stateDef = FieldDefinition.CreateState("NonExistentProvince");
        var match = ElementFinder.FindField(stateDef);

        Assert.That(match.IsSuccessfulMatch, Is.True);

        var actions = new DropdownActions(Driver);
        var ex = Assert.Throws<NoSuchElementException>(() =>
        {
            actions.SelectOption(match.Candidate!, "NonExistentProvince");
        });

        Assert.That(ex!.Message, Does.Contain("not found in dropdown"));
        Assert.That(ex.Message, Does.Contain("California"));
    }

    [Test]
    public void Mutation8_ScenarioE_DynamicallyRenderedFields_ShouldIdentifyViaExplicitWait()
    {
        string url = GetFixtureUrl("dynamic-fields.html");
        BrowserManager.NavigateTo(url);

        // The fixture renders fields via setTimeout(..., 1200ms)
        // ElementFinder with explicit wait polling should discover and interact successfully
        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var fnResult = ElementFinder.FindField(fnDef, timeoutSeconds: 5);

        Assert.That(fnResult.IsSuccessfulMatch, Is.True,
            $"Expected dynamic field discovery after async rendering. Reason: {fnResult.DiagnosticExplanation}");

        var actions = new TextFieldActions(Driver);
        string actual = actions.SetText(fnResult.Candidate!, "Shiva");
        Assert.That(actual, Is.EqualTo("Shiva"));
    }

    [Test]
    public void Mutation8_ScenarioF_StaleElementRecovery_ShouldReInspectAndSucceed()
    {
        string url = GetFixtureUrl("baseline.html");
        BrowserManager.NavigateTo(url);

        var fnDef = FieldDefinition.CreateFirstName("Shiva");
        var match1 = ElementFinder.FindField(fnDef);
        Assert.That(match1.IsSuccessfulMatch, Is.True);

        // Simulate a DOM replacement of the element (e.g., client-side framework re-render)
        ((IJavaScriptExecutor)Driver).ExecuteScript(@"
            var oldInput = document.getElementById('fname');
            var parent = oldInput.parentNode;
            var newInput = oldInput.cloneNode(true);
            parent.replaceChild(newInput, oldInput);
        ");

        // Old WebElement reference is now detached and stale
        Assert.Throws<StaleElementReferenceException>(() => _ = match1.Candidate!.Element.Enabled);

        // Framework re-inspects DOM to obtain fresh reference
        var freshMatch = ElementFinder.FindField(fnDef);
        Assert.That(freshMatch.IsSuccessfulMatch, Is.True);
        Assert.That(freshMatch.Candidate!.Element, Is.Not.EqualTo(match1.Candidate!.Element));

        var actions = new TextFieldActions(Driver);
        string actual = actions.SetText(freshMatch.Candidate, "Shiva");
        Assert.That(actual, Is.EqualTo("Shiva"));
    }

    [Test]
    public void Mutation8_ScenarioG_ElementNotInteractable_ShouldFailSafely()
    {
        string url = GetFixtureUrl("interaction-failures.html");
        BrowserManager.NavigateTo(url);

        var hiddenCandidate = new ElementCandidate
        {
            Element = Driver.FindElement(By.Id("hidden_fname")),
            TagName = "input",
            InputType = "text",
            IsVisible = false,
            IsEnabled = true
        };

        var actions = new TextFieldActions(Driver);
        Assert.Throws<ElementNotInteractableException>(() =>
        {
            actions.SetText(hiddenCandidate, "TestValue");
        });
    }

    [Test]
    public void Mutation8_ScenarioH_BrowserFailureAndCleanup_ShouldHandleGracefully()
    {
        // Test navigation to invalid URL
        Assert.Throws<WebDriverException>(() =>
        {
            BrowserManager.NavigateTo("http://this-is-an-invalid-domain-that-does-not-exist.local");
        });

        // Verify session can still be disposed cleanly without orphan processes
        Assert.DoesNotThrow(() =>
        {
            BrowserManager.Dispose();
        });
    }
}

