using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveAutomation.Models;
using AdaptiveAutomation.Utilities;

namespace AdaptiveAutomation.Discovery;

public static class SemanticMatcher
{
    public static bool HasContradictoryKeyword(string text, IEnumerable<string> negativeKeywords)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        string normText = TextNormalizer.Normalize(text);
        foreach (var neg in negativeKeywords)
        {
            string normNeg = TextNormalizer.Normalize(neg);
            if (!string.IsNullOrEmpty(normNeg) && normText.Contains(normNeg, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static double MatchSynonyms(string? candidateSignal, IReadOnlyList<string> synonyms)
    {
        if (string.IsNullOrWhiteSpace(candidateSignal) || synonyms.Count == 0)
        {
            return 0.0;
        }

        string normSignal = TextNormalizer.Normalize(candidateSignal);
        if (string.IsNullOrEmpty(normSignal))
        {
            return 0.0;
        }

        double maxScore = 0.0;
        foreach (var synonym in synonyms)
        {
            string normSynonym = TextNormalizer.Normalize(synonym);
            if (string.IsNullOrEmpty(normSynonym)) continue;

            // 1. Exact normalized match: 1.0
            if (string.Equals(normSignal, normSynonym, StringComparison.OrdinalIgnoreCase))
            {
                return 1.0;
            }

            // 2. Starts with or ends with exact synonym phrase
            if (normSignal.StartsWith(normSynonym + " ", StringComparison.OrdinalIgnoreCase) ||
                normSignal.EndsWith(" " + normSynonym, StringComparison.OrdinalIgnoreCase))
            {
                maxScore = Math.Max(maxScore, 0.90);
            }
            // 3. Contains synonym phrase
            else if (normSignal.Contains(normSynonym, StringComparison.OrdinalIgnoreCase))
            {
                maxScore = Math.Max(maxScore, 0.80);
            }
            // 4. Token overlap
            else
            {
                double overlap = TextNormalizer.TokenOverlapScore(normSignal, normSynonym);
                maxScore = Math.Max(maxScore, overlap * 0.70);
            }
        }

        return maxScore;
    }
}

