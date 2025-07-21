using System;

namespace InnoAndLogic.Shared;

/// <summary>
/// Provides extension methods for working with strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Compares a nullable string to a non-nullable string, treating null as equal to an empty string.
    /// </summary>
    /// <param name="nullableString">The nullable string to compare.</param>
    /// <param name="otherString">The non-nullable string to compare against.</param>
    /// <returns>
    /// True if the strings are equal (considering null as equal to an empty string); otherwise, false.
    /// </returns>
    public static bool EqualsNullOrEmpty(this string? nullableString, string otherString) 
        => (nullableString ?? string.Empty) == otherString;

    // Contains
    /// <summary>
    /// Determines whether the string contains the specified substring using invariant culture comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The substring to check for.</param>
    /// <returns>True if the source string contains the specified substring; otherwise, false.</returns>
    public static bool ContainsInvariant(this string source, string value) 
        => source.Contains(value, StringComparison.InvariantCulture);

    /// <summary>
    /// Determines whether the string contains the specified substring using invariant culture, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The substring to check for.</param>
    /// <returns>True if the source string contains the specified substring; otherwise, false.</returns>
    public static bool ContainsInvariantIgnoreCase(this string source, string value) 
        => source.Contains(value, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the string contains the specified substring using ordinal comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The substring to check for.</param>
    /// <returns>True if the source string contains the specified substring; otherwise, false.</returns>
    public static bool ContainsOrdinal(this string source, string value) 
        => source.Contains(value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the string contains the specified substring using ordinal comparison, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The substring to check for.</param>
    /// <returns>True if the source string contains the specified substring; otherwise, false.</returns>
    public static bool ContainsOrdinalIgnoreCase(this string source, string value) 
        => source.Contains(value, StringComparison.OrdinalIgnoreCase);

    // StartsWith
    /// <summary>
    /// Determines whether the string starts with the specified prefix using invariant culture comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="prefix">The prefix to check for.</param>
    /// <returns>True if the source string starts with the specified prefix; otherwise, false.</returns>
    public static bool StartsWithInvariant(this string source, string prefix) 
        => source.StartsWith(prefix, StringComparison.InvariantCulture);

    /// <summary>
    /// Determines whether the string starts with the specified prefix using invariant culture, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="prefix">The prefix to check for.</param>
    /// <returns>True if the source string starts with the specified prefix; otherwise, false.</returns>
    public static bool StartsWithInvariantIgnoreCase(this string source, string prefix) 
        => source.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the string starts with the specified prefix using ordinal comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="prefix">The prefix to check for.</param>
    /// <returns>True if the source string starts with the specified prefix; otherwise, false.</returns>
    public static bool StartsWithOrdinal(this string source, string prefix) 
        => source.StartsWith(prefix, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the string starts with the specified prefix using ordinal comparison, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="prefix">The prefix to check for.</param>
    /// <returns>True if the source string starts with the specified prefix; otherwise, false.</returns>
    public static bool StartsWithOrdinalIgnoreCase(this string source, string prefix) 
        => source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

    // EndsWith
    /// <summary>
    /// Determines whether the string ends with the specified suffix using invariant culture comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="suffix">The suffix to check for.</param>
    /// <returns>True if the source string ends with the specified suffix; otherwise, false.</returns>
    public static bool EndsWithInvariant(this string source, string suffix) 
        => source.EndsWith(suffix, StringComparison.InvariantCulture);

    /// <summary>
    /// Determines whether the string ends with the specified suffix using invariant culture, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="suffix">The suffix to check for.</param>
    /// <returns>True if the source string ends with the specified suffix; otherwise, false.</returns>
    public static bool EndsWithInvariantIgnoreCase(this string source, string suffix) 
        => source.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the string ends with the specified suffix using ordinal comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="suffix">The suffix to check for.</param>
    /// <returns>True if the source string ends with the specified suffix; otherwise, false.</returns>
    public static bool EndsWithOrdinal(this string source, string suffix) 
        => source.EndsWith(suffix, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the string ends with the specified suffix using ordinal comparison, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="suffix">The suffix to check for.</param>
    /// <returns>True if the source string ends with the specified suffix; otherwise, false.</returns>
    public static bool EndsWithOrdinalIgnoreCase(this string source, string suffix) 
        => source.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);

    // Equals
    /// <summary>
    /// Determines whether the string is equal to the specified string using invariant culture comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="other">The string to compare against.</param>
    /// <returns>True if the strings are equal; otherwise, false.</returns>
    public static bool EqualsInvariant(this string source, string other) 
        => string.Equals(source, other, StringComparison.InvariantCulture);

    /// <summary>
    /// Determines whether the string is equal to the specified string using invariant culture, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="other">The string to compare against.</param>
    /// <returns>True if the strings are equal; otherwise, false.</returns>
    public static bool EqualsInvariantIgnoreCase(this string source, string other) 
        => string.Equals(source, other, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the string is equal to the specified string using ordinal comparison.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="other">The string to compare against.</param>
    /// <returns>True if the strings are equal; otherwise, false.</returns>
    public static bool EqualsOrdinal(this string source, string other) 
        => string.Equals(source, other, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the string is equal to the specified string using ordinal comparison, ignoring case.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="other">The string to compare against.</param>
    /// <returns>True if the strings are equal; otherwise, false.</returns>
    public static bool EqualsOrdinalIgnoreCase(this string source, string other) 
        => string.Equals(source, other, StringComparison.OrdinalIgnoreCase);
}
