using System.Collections.Generic;

namespace AdaptiveAutomation.Models;

public record ElementMatchResult
{
    public required FieldDefinition Field { get; init; }
    public ElementCandidate? Candidate { get; init; }
    public double Score { get; init; }
    public ConfidenceLevel Confidence { get; init; }
    public bool IsAmbiguous { get; init; }
    public double AmbiguityScoreDelta { get; init; }
    public IReadOnlyList<ElementCandidate> CompetitorCandidates { get; init; } = new List<ElementCandidate>();
    public IReadOnlyList<ElementCandidate> RejectedCandidates { get; init; } = new List<ElementCandidate>();
    public string DiagnosticExplanation { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, double> SignalBreakdown { get; init; } = new Dictionary<string, double>();

    public bool IsSuccessfulMatch => Candidate != null && !IsAmbiguous && Confidence >= ConfidenceLevel.Medium;
}

