using System;
using AdaptiveAutomation.Discovery;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Utilities;

namespace AdaptiveAutomation.Scoring;

public class ElementScorer
{
    private readonly ScoringWeights _weights;

    public ElementScorer(ScoringWeights? weights = null)
    {
        _weights = weights ?? new ScoringWeights();
    }

    public double Score(FieldDefinition field, ElementCandidate candidate)
    {
        candidate.MatchEvidence.Clear();
        double totalScore = 0.0;

        // 1. Check for contradictory negative keywords first (e.g., contains 'Last' or 'Company' when searching 'First Name')
        bool hasContradictory =
            SemanticMatcher.HasContradictoryKeyword(candidate.AssociatedLabelText, field.NegativeKeywords) ||
            SemanticMatcher.HasContradictoryKeyword(candidate.EnclosingLabelText, field.NegativeKeywords) ||
            SemanticMatcher.HasContradictoryKeyword(candidate.Placeholder, field.NegativeKeywords) ||
            SemanticMatcher.HasContradictoryKeyword(candidate.Name, field.NegativeKeywords) ||
            SemanticMatcher.HasContradictoryKeyword(candidate.Id, field.NegativeKeywords);

        if (hasContradictory)
        {
            candidate.MatchEvidence["ContradictoryPenalty"] = -_weights.ContradictoryPenalty;
            totalScore -= _weights.ContradictoryPenalty;
        }

        // 2. Direct Label Match (Associated Label or Enclosing Label, deduplicated)
        double directLabelScore = 0.0;
        if (!string.IsNullOrWhiteSpace(candidate.AssociatedLabelText))
        {
            double matchRatio = SemanticMatcher.MatchSynonyms(candidate.AssociatedLabelText, field.Synonyms);
            if (matchRatio > 0)
            {
                directLabelScore = matchRatio * _weights.AssociatedLabelWeight;
                candidate.MatchEvidence["AssociatedLabelMatch"] = directLabelScore;

                if (matchRatio >= 0.8 && !candidate.MatchEvidence.ContainsKey("ContainerRelevance"))
                {
                    double containerPoints = matchRatio * _weights.ContainerRelevanceWeight;
                    candidate.MatchEvidence["ContainerRelevance"] = containerPoints;
                    totalScore += containerPoints;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(candidate.EnclosingLabelText))
        {
            double matchRatio = SemanticMatcher.MatchSynonyms(candidate.EnclosingLabelText, field.Synonyms);
            if (matchRatio > 0)
            {
                double enclosingScore = matchRatio * _weights.AssociatedLabelWeight;
                candidate.MatchEvidence["EnclosingLabelMatch"] = enclosingScore;
                directLabelScore = Math.Max(directLabelScore, enclosingScore);

                if (matchRatio >= 0.8 && !candidate.MatchEvidence.ContainsKey("ContainerRelevance"))
                {
                    double containerPoints = matchRatio * _weights.ContainerRelevanceWeight;
                    candidate.MatchEvidence["ContainerRelevance"] = containerPoints;
                    totalScore += containerPoints;
                }
            }
        }
        totalScore += directLabelScore;

        // 3. Accessible Name / ARIA Match
        string ariaSignal = !string.IsNullOrWhiteSpace(candidate.AriaLabel)
            ? candidate.AriaLabel
            : candidate.AriaLabelledByText;

        if (!string.IsNullOrWhiteSpace(ariaSignal))
        {
            double matchRatio = SemanticMatcher.MatchSynonyms(ariaSignal, field.Synonyms);
            if (matchRatio > 0)
            {
                double points = matchRatio * _weights.AccessibleNameWeight;
                candidate.MatchEvidence["AccessibleNameMatch"] = points;
                totalScore += points;

                if (matchRatio >= 0.8 && !candidate.MatchEvidence.ContainsKey("ContainerRelevance"))
                {
                    double containerPoints = matchRatio * _weights.ContainerRelevanceWeight;
                    candidate.MatchEvidence["ContainerRelevance"] = containerPoints;
                    totalScore += containerPoints;
                }
            }
        }

        // 4. Contextual Proximity Text & Container Relevance Match
        if (!string.IsNullOrWhiteSpace(candidate.NearbyContextText))
        {
            double matchRatio = SemanticMatcher.MatchSynonyms(candidate.NearbyContextText, field.Synonyms);
            if (matchRatio > 0)
            {
                double points = matchRatio * _weights.ContextualTextWeight;
                candidate.MatchEvidence["ContextualTextMatch"] = points;
                totalScore += points;

                if (matchRatio >= 0.8 && !candidate.MatchEvidence.ContainsKey("ContainerRelevance"))
                {
                    double containerPoints = matchRatio * _weights.ContainerRelevanceWeight;
                    candidate.MatchEvidence["ContainerRelevance"] = containerPoints;
                    totalScore += containerPoints;
                }
            }
        }

        // 5. Placeholder Match
        if (!string.IsNullOrWhiteSpace(candidate.Placeholder))
        {
            double matchRatio = SemanticMatcher.MatchSynonyms(candidate.Placeholder, field.Synonyms);
            if (matchRatio > 0)
            {
                double points = matchRatio * _weights.PlaceholderWeight;
                candidate.MatchEvidence["PlaceholderMatch"] = points;
                totalScore += points;
            }
        }

        // 6. Name or ID attribute semantic fallback
        string attrSignal = $"{candidate.Name} {candidate.Id}".Trim();
        if (!string.IsNullOrWhiteSpace(attrSignal))
        {
            double matchRatio = SemanticMatcher.MatchSynonyms(attrSignal, field.Synonyms);
            if (matchRatio > 0)
            {
                double points = matchRatio * _weights.AttributeNameWeight;
                candidate.MatchEvidence["AttributeNameMatch"] = points;
                totalScore += points;
            }
        }

        // 7. Technical Type Match
        bool typeMatches = field.FieldType switch
        {
            FieldType.TextInput => candidate.TagName.Equals("input", StringComparison.OrdinalIgnoreCase) &&
                                  (candidate.InputType.Equals("text", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(candidate.InputType)),
            FieldType.RadioGroup => candidate.InputType.Equals("radio-group", StringComparison.OrdinalIgnoreCase),
            FieldType.Dropdown => candidate.TagName.Equals("select", StringComparison.OrdinalIgnoreCase),
            _ => false
        };

        if (typeMatches)
        {
            candidate.MatchEvidence["TechnicalTypeMatch"] = _weights.TechnicalTypeWeight;
            totalScore += _weights.TechnicalTypeWeight;
        }

        // Bound final score between 0 and 100
        double finalScore = Math.Max(0.0, Math.Min(100.0, totalScore));
        return Math.Round(finalScore, 2);
    }
}

