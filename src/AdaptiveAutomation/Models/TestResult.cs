using System;

namespace AdaptiveAutomation.Models;

public record TestResult
{
    public required string TestName { get; init; }
    public required string FieldName { get; init; }
    public required string TargetUrl { get; init; }
    public DateTime ExecutionTimestamp { get; init; } = DateTime.UtcNow;
    public TestStatus Status { get; init; }
    public double ConfidenceScore { get; init; }
    public string MatchingStrategy { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public string ActualValue { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DiagnosticDetails { get; init; }
    public string? ScreenshotPath { get; init; }
}

