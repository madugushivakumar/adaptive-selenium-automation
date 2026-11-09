using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdaptiveAutomation.Utilities;

public static class TextNormalizer
{
    private static readonly Regex MultipleSpacesRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);

    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // 1. Lowercase invariant
        string lower = input.ToLowerInvariant();

        // 2. Replace hyphens and underscores with spaces to preserve word boundaries
        string replaced = lower.Replace('-', ' ').Replace('_', ' ');

        // 3. Remove other punctuation (colons, asterisks, etc.)
        string clean = PunctuationRegex.Replace(replaced, " ");

        // 4. Collapse multiple spaces and trim
        return MultipleSpacesRegex.Replace(clean, " ").Trim();
    }

    public static bool ContainsNormalized(string? source, string? target)
    {
        string normSource = Normalize(source);
        string normTarget = Normalize(target);

        if (string.IsNullOrEmpty(normSource) || string.IsNullOrEmpty(normTarget))
        {
            return false;
        }

        return normSource.Contains(normTarget, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ExactMatch(string? a, string? b)
    {
        string normA = Normalize(a);
        string normB = Normalize(b);

        if (string.IsNullOrEmpty(normA) || string.IsNullOrEmpty(normB))
        {
            return false;
        }

        return string.Equals(normA, normB, StringComparison.OrdinalIgnoreCase);
    }

    public static double TokenOverlapScore(string? candidateText, string? targetText)
    {
        string normCandidate = Normalize(candidateText);
        string normTarget = Normalize(targetText);

        if (string.IsNullOrEmpty(normCandidate) || string.IsNullOrEmpty(normTarget))
        {
            return 0.0;
        }

        var candidateTokens = new HashSet<string>(normCandidate.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var targetTokens = new HashSet<string>(normTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (candidateTokens.Count == 0 || targetTokens.Count == 0)
        {
            return 0.0;
        }

        int matchingCount = targetTokens.Count(t => candidateTokens.Contains(t));
        return (double)matchingCount / targetTokens.Count;
    }
}

