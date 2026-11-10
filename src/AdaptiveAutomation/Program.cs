using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AdaptiveAutomation.Actions;
using AdaptiveAutomation.Browser;
using AdaptiveAutomation.Configuration;
using AdaptiveAutomation.Discovery;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Reporting;
using AdaptiveAutomation.Utilities;
using Microsoft.Extensions.Logging;

namespace AdaptiveAutomation;

public class Program
{
    public static int Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("======================================================================");
        Console.WriteLine("   Adaptive Selenium Web Form Automation Engine (.NET 8)");
        Console.WriteLine("======================================================================");
        Console.ResetColor();

        var settings = ConfigurationLoader.Load(args);

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = false;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger("AdaptiveAutomation");

        logger.LogInformation("Target URL: {Url}", settings.Application.BaseUrl);
        logger.LogInformation("Headless Mode: {Headless}", settings.Browser.Headless);
        logger.LogInformation("Test Data: FirstName='{FirstName}', Gender='{Gender}', State='{State}'",
            settings.TestData.FirstName, settings.TestData.Gender, settings.TestData.State);

        var results = new List<TestResult>();
        using var browser = new BrowserManager(settings, logger);

        try
        {
            browser.NavigateTo(settings.Application.BaseUrl);
            var finder = new ElementFinder(browser.Driver, settings, logger);

            // Test 1: First Name
            results.Add(ExecuteFieldTest("Test_FirstName", "First Name", () =>
            {
                var fieldDef = FieldDefinition.CreateFirstName(settings.TestData.FirstName);
                var match = finder.FindField(fieldDef);
                if (!match.IsSuccessfulMatch)
                {
                    throw new InvalidOperationException($"Identification failed for '{fieldDef.FieldName}'. Score: {match.Score:F1}. {match.DiagnosticExplanation}");
                }

                var actions = new TextFieldActions(browser.Driver, logger);
                string actual = actions.SetText(match.Candidate!, settings.TestData.FirstName);

                return (match.Score, "Associated Label / Semantic Heuristic", settings.TestData.FirstName, actual);
            }, settings.Application.BaseUrl, browser, logger));

            // Test 2: Gender
            results.Add(ExecuteFieldTest("Test_Gender", "Gender", () =>
            {
                var fieldDef = FieldDefinition.CreateGender(settings.TestData.Gender);
                var match = finder.FindField(fieldDef);
                if (!match.IsSuccessfulMatch)
                {
                    throw new InvalidOperationException($"Identification failed for '{fieldDef.FieldName}'. Score: {match.Score:F1}. {match.DiagnosticExplanation}");
                }

                var actions = new RadioGroupActions(browser.Driver, logger);
                string actual = actions.SelectOption(match.Candidate!, settings.TestData.Gender);

                return (match.Score, "Logical Grouping / Proximity Analysis", settings.TestData.Gender, actual);
            }, settings.Application.BaseUrl, browser, logger));

            // Test 3: State
            results.Add(ExecuteFieldTest("Test_State", "State", () =>
            {
                var fieldDef = FieldDefinition.CreateState(settings.TestData.State);
                var match = finder.FindField(fieldDef);
                if (!match.IsSuccessfulMatch)
                {
                    throw new InvalidOperationException($"Identification failed for '{fieldDef.FieldName}'. Score: {match.Score:F1}. {match.DiagnosticExplanation}");
                }

                var actions = new DropdownActions(browser.Driver, logger);
                string actual = actions.SelectOption(match.Candidate!, settings.TestData.State);

                return (match.Score, "Select Semantic Discovery / Option Matching", settings.TestData.State, actual);
            }, settings.Application.BaseUrl, browser, logger));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during automation run.");
        }
        finally
        {
            browser.Dispose();
        }

        // Generate Reports
        string jsonReport = JsonReportGenerator.Generate(settings.Application.BaseUrl, results);
        string htmlReport = HtmlReportGenerator.Generate(settings.Application.BaseUrl, results);

        // Print Summary Table
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine("                        EXECUTION SUMMARY                             ");
        Console.WriteLine("----------------------------------------------------------------------");
        Console.ResetColor();

        int passed = 0;
        foreach (var r in results)
        {
            // Verify BOTH Status == TestStatus.Passed AND that actual value strictly matches expected value
            bool valueVerified = r.Status == TestStatus.Passed &&
                                 !string.IsNullOrEmpty(r.ExpectedValue) &&
                                 !string.IsNullOrEmpty(r.ActualValue) &&
                                 TextNormalizer.IsMatchForValue(r.ActualValue, r.ExpectedValue);

            if (valueVerified)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASS] {r.FieldName,-12} | Conf: {r.ConfidenceScore,5:F1}% | Expected: {r.ExpectedValue,-10} | Actual: {r.ActualValue,-10} ({r.Duration.TotalSeconds:F2}s)");
                passed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string error = r.ErrorMessage ?? $"Value mismatch: Expected '{r.ExpectedValue}', got '{r.ActualValue}'";
                Console.WriteLine($"[FAIL] {r.FieldName,-12} | Status: Failed | Expected: {r.ExpectedValue,-10} | Actual: {r.ActualValue,-10} | Error: {error}");
            }
        }
        Console.ResetColor();
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine($"Results: {passed}/{results.Count} Passed.");
        Console.WriteLine($"Reports Generated:\n  - JSON: {jsonReport}\n  - HTML: {htmlReport}");
        Console.WriteLine("----------------------------------------------------------------------");

        // Fail with non-zero exit code if not all 3 target fields passed with verified values
        bool allPassed = (passed == 3 && results.Count == 3);
        return allPassed ? 0 : 1;
    }

    private static TestResult ExecuteFieldTest(
        string testName,
        string fieldName,
        Func<(double Confidence, string Strategy, string Expected, string Actual)> testAction,
        string targetUrl,
        BrowserManager browser,
        ILogger logger)
    {
        var sw = Stopwatch.StartNew();
        string capturedExpected = string.Empty;
        string capturedActual = string.Empty;
        double capturedConfidence = 0.0;
        string capturedStrategy = "N/A";

        try
        {
            var (confidence, strategy, expected, actual) = testAction();
            capturedConfidence = confidence;
            capturedStrategy = strategy;
            capturedExpected = expected;
            capturedActual = actual;
            sw.Stop();

            // Strict value assertion
            bool valueMatched = TextNormalizer.IsMatchForValue(actual, expected);
            if (!valueMatched)
            {
                throw new InvalidOperationException($"Value verification failed for '{fieldName}'. Expected: '{expected}', Actual: '{actual}'.");
            }

            return new TestResult
            {
                TestName = testName,
                FieldName = fieldName,
                TargetUrl = targetUrl,
                Status = TestStatus.Passed,
                ConfidenceScore = confidence,
                MatchingStrategy = strategy,
                ExpectedValue = expected,
                ActualValue = actual,
                Duration = sw.Elapsed
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Execution failed for field '{FieldName}'.", fieldName);
            string? screenshotPath = browser.CaptureScreenshot(fieldName);
            return new TestResult
            {
                TestName = testName,
                FieldName = fieldName,
                TargetUrl = targetUrl,
                Status = ex.Message.Contains("Ambiguous match") ? TestStatus.Ambiguous : TestStatus.Failed,
                ConfidenceScore = capturedConfidence,
                MatchingStrategy = capturedStrategy,
                ExpectedValue = capturedExpected,
                ActualValue = capturedActual,
                ErrorMessage = ex.Message,
                Duration = sw.Elapsed,
                ScreenshotPath = screenshotPath
            };
        }
    }
}

