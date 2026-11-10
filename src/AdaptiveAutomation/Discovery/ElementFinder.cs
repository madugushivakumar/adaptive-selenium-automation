using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveAutomation.Configuration;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Scoring;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace AdaptiveAutomation.Discovery;

public class ElementFinder
{
    private readonly IWebDriver _driver;
    private readonly DomInspector _inspector;
    private readonly ElementScorer _scorer;
    private readonly ConfidenceEvaluator _evaluator;
    private readonly ILogger? _logger;

    public ElementFinder(IWebDriver driver, AppSettings settings, ILogger? logger = null)
    {
        _driver = driver;
        _inspector = new DomInspector(driver);
        _scorer = new ElementScorer();
        _evaluator = new ConfidenceEvaluator(settings.Matching);
        _logger = logger;
    }

    public ElementMatchResult FindField(FieldDefinition field, int timeoutSeconds = 3)
    {
        _logger?.LogInformation("Locating field '{FieldName}' of type {FieldType}...", field.FieldName, field.FieldType);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        ElementMatchResult lastResult = null!;

        do
        {
            var candidates = _inspector.InspectCandidates(field.FieldType);
            _logger?.LogDebug("Inspected {Count} candidates for field '{FieldName}'.", candidates.Count, field.FieldName);

            var scored = new List<(ElementCandidate Candidate, double Score)>();
            foreach (var candidate in candidates)
            {
                double score = _scorer.Score(field, candidate);
                scored.Add((candidate, score));
                _logger?.LogDebug("Candidate {Candidate} scored: {Score}", candidate, score);
            }

            lastResult = _evaluator.Evaluate(field, scored);

            if (lastResult.IsSuccessfulMatch || lastResult.IsAmbiguous)
            {
                break;
            }

            if (timeoutSeconds > 0 && sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                System.Threading.Thread.Sleep(200);
            }
        } while (sw.Elapsed.TotalSeconds < timeoutSeconds);

        _logger?.LogInformation("Match result for '{FieldName}': Score={Score}, Confidence={Confidence}, Ambiguous={IsAmbiguous}",
            field.FieldName, lastResult.Score, lastResult.Confidence, lastResult.IsAmbiguous);

        return lastResult;
    }
}

