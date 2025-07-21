using Xunit;

namespace InnoAndLogic.Shared.Tests;

public class StringExtensionsTests {
    [Theory]
    [InlineData(null, "", true)]
    [InlineData("", "", true)]
    [InlineData("test", "test", true)]
    [InlineData("test", "TEST", false)]
    public void EqualsNullOrEmpty_ShouldReturnExpectedResult(string? nullableString, string otherString, bool expected) {
        // Act
        bool result = nullableString.EqualsNullOrEmpty(otherString);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "world", true)]
    [InlineData("hello world", "WORLD", false)]
    public void ContainsInvariant_ShouldReturnExpectedResult(string source, string value, bool expected) {
        // Act
        bool result = source.ContainsInvariant(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "WORLD", true)]
    [InlineData("hello world", "planet", false)]
    public void ContainsInvariantIgnoreCase_ShouldReturnExpectedResult(string source, string value, bool expected) {
        // Act
        bool result = source.ContainsInvariantIgnoreCase(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "he", true)]
    [InlineData("hello", "HE", false)]
    public void StartsWithInvariant_ShouldReturnExpectedResult(string source, string prefix, bool expected) {
        // Act
        bool result = source.StartsWithInvariant(prefix);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "HE", true)]
    [InlineData("hello", "world", false)]
    public void StartsWithInvariantIgnoreCase_ShouldReturnExpectedResult(string source, string prefix, bool expected) {
        // Act
        bool result = source.StartsWithInvariantIgnoreCase(prefix);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "lo", true)]
    [InlineData("hello", "LO", false)]
    public void EndsWithInvariant_ShouldReturnExpectedResult(string source, string suffix, bool expected) {
        // Act
        bool result = source.EndsWithInvariant(suffix);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "LO", true)]
    [InlineData("hello", "world", false)]
    public void EndsWithInvariantIgnoreCase_ShouldReturnExpectedResult(string source, string suffix, bool expected) {
        // Act
        bool result = source.EndsWithInvariantIgnoreCase(suffix);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "hello", true)]
    [InlineData("hello", "HELLO", false)]
    public void EqualsInvariant_ShouldReturnExpectedResult(string source, string other, bool expected) {
        // Act
        bool result = source.EqualsInvariant(other);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "HELLO", true)]
    [InlineData("hello", "world", false)]
    public void EqualsInvariantIgnoreCase_ShouldReturnExpectedResult(string source, string other, bool expected) {
        // Act
        bool result = source.EqualsInvariantIgnoreCase(other);

        // Assert
        Assert.Equal(expected, result);
    }
}