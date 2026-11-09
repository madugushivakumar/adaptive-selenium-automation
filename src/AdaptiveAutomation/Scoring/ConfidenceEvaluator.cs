using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveAutomation.Configuration;
using AdaptiveAutomation.Models;

namespace AdaptiveAutomation.Scoring;

public class ConfidenceEvaluator
{
    private readonly MatchingSettings _settings;

    public ConfidenceEvaluator(MatchingSettings? settings = null)
    {
        _settings = settings ?? new MatchingSettings();
    }

    public ElementMatchResult Evaluate(FieldDefinition field, IReadOnlyList<(ElementCandidate Candidate, double Score)> scoredCandidates)
    {
        if (scoredCandidates.Count == 0)
        {
            return new ElementMatchResult
            {
                Field = field,
                Candidate = null,
                Score = 0,
                Confidence = ConfidenceLevel.Rejected,
                IsAmbiguous = false,
                DiagnosticExplanation = $"No candidates found on the page matching technical type '{field.FieldType}' for field '{field.FieldName}'."
            };
        }

        // Order by score descending
        var sorted = scoredCandidates.OrderByDescending(x => x.Score).ToList();
        var top = sorted[0];

        // 1. Check if top score meets minimum confidence threshold
        if (top.Score < _settings.MinimumConfidenceScore)
        {
            return new ElementMatchResult
            {
                Field = field,
                Candidate = top.Candidate,
                Score = top.Score,
                Confidence = ConfidenceLevel.Low,
                IsAmbiguous = false,
                RejectedCandidates = sorted.Skip(1).Select(x => x.Candidate).ToList(),
                DiagnosticExplanation = $"Highest candidate scored {top.Score:F1}, which is below the minimum confidence threshold {_settings.MinimumConfidenceScore:F1}. Candidate: {top.Candidate}",
                SignalBreakdown = top.Candidate.MatchEvidence
            };
        }

        // 2. Check for Ambiguity against second-place candidate
        if (sorted.Count > 1)
        {
            var runnerUp = sorted[1];
            double delta = Math.Abs(top.Score - runnerUp.Score);

            // If the runner-up is also above or very close to threshold and delta is within margin
            if (runnerUp.Score >= _settings.MinimumConfidenceScore - 10 && delta <= _settings.AmbiguityMargin)
            {
                return new ElementMatchResult
                {
                    Field = field,
                    Candidate = top.Candidate,
                    Score = top.Score,
                    Confidence = ConfidenceLevel.Rejected,
                    IsAmbiguous = true,
                    AmbiguityScoreDelta = delta,
                    CompetitorCandidates = new[] { top.Candidate, runnerUp.Candidate },
                    RejectedCandidates = sorted.Skip(2).Select(x => x.Candidate).ToList(),
                    DiagnosticExplanation = $"Ambiguous match detected between candidate 1 (Score: {top.Score:F1}) and candidate 2 (Score: {runnerUp.Score:F1}) with score difference {delta:F1} within margin {_settings.AmbiguityMargin:F1}. Refusing arbitrary selection.",
                    SignalBreakdown = top.Candidate.MatchEvidence
                };
            }
        }

        // 3. High or Medium Confidence
        ConfidenceLevel level = top.Score >= 80.0 ? ConfidenceLevel.High : ConfidenceLevel.Medium;

        return new ElementMatchResult
        {
            Field = field,
            Candidate = top.Candidate,
            Score = top.Score,
            Confidence = level,
            IsAmbiguous = false,
            RejectedCandidates = sorted.Skip(1).Select(x => x.Candidate).ToList(),
            DiagnosticExplanation = $"Successfully identified '{field.FieldName}' with {level} confidence (Score: {top.Score:F1}). Selected: {top.Candidate}",
            SignalBreakdown = top.Candidate.MatchEvidence
        };
    }
}

