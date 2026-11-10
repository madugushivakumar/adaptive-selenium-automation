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

    public static bool MatchesWord(string? source, string? target)
    {
        string normSource = Normalize(source);
        string normTarget = Normalize(target);

        if (string.IsNullOrEmpty(normSource) || string.IsNullOrEmpty(normTarget))
        {
            return false;
        }

        if (string.Equals(normSource, normTarget, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Regex.IsMatch(normSource, $@"\b{Regex.Escape(normTarget)}\b", RegexOptions.IgnoreCase);
    }

    public static bool IsMatchForValue(string? actual, string? expected)
    {
        string normActual = Normalize(actual);
        string normExpected = Normalize(expected);

        if (string.IsNullOrEmpty(normActual) || string.IsNullOrEmpty(normExpected))
        {
            return false;
        }

        // 1. Exact normalized match
        if (string.Equals(normActual, normExpected, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 2. Full word boundary match (e.g. "Male (Option)" matches "Male", "India - IN" matches "India")
        // Word boundary \b ensures that "Female" will NEVER match "Male"
        return Regex.IsMatch(normActual, $@"\b{Regex.Escape(normExpected)}\b", RegexOptions.IgnoreCase);
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

