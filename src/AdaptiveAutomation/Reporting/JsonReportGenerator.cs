using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdaptiveAutomation.Models;

namespace AdaptiveAutomation.Reporting;

public class ExecutionSummaryReport
{
    public string TestSuite { get; set; } = "Adaptive Form Automation";
    public string TargetUrl { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int AmbiguousTests { get; set; }
    public double TotalDurationSeconds { get; set; }
    public List<TestResultItem> Results { get; set; } = new();
}

public class TestResultItem
{
    public string TestName { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TestStatus Status { get; set; }
    public double Confidence { get; set; }
    public string MatchingStrategy { get; set; } = string.Empty;
    public string ExpectedValue { get; set; } = string.Empty;
    public string ActualValue { get; set; } = string.Empty;
    public double DurationSeconds { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ScreenshotPath { get; set; }
}

public static class JsonReportGenerator
{
    public static string Generate(string targetUrl, IEnumerable<TestResult> results, string? outputPath = null)
    {
        outputPath ??= Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "reports", "latest-report.json");
        string? dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var resultList = new List<TestResult>(results);
        var report = new ExecutionSummaryReport
        {
            TargetUrl = targetUrl,
            TotalTests = resultList.Count,
            PassedTests = resultList.Count(r => r.Status == TestStatus.Passed),
            FailedTests = resultList.Count(r => r.Status == TestStatus.Failed),
            AmbiguousTests = resultList.Count(r => r.Status == TestStatus.Ambiguous),
            TotalDurationSeconds = resultList.Sum(r => r.Duration.TotalSeconds),
            Results = resultList.Select(r => new TestResultItem
            {
                TestName = r.TestName,
                Field = r.FieldName,
                Status = r.Status,
                Confidence = Math.Round(r.ConfidenceScore, 1),
                MatchingStrategy = r.MatchingStrategy,
                ExpectedValue = r.ExpectedValue,
                ActualValue = r.ActualValue,
                DurationSeconds = Math.Round(r.Duration.TotalSeconds, 2),
                ErrorMessage = r.ErrorMessage,
                ScreenshotPath = r.ScreenshotPath
            }).ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string json = JsonSerializer.Serialize(report, options);
        File.WriteAllText(outputPath, json);
        return outputPath;
    }
}

