using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;

namespace ValidationFramework.Tests;

public class StringValidatorTests
{
    [Theory]
    [InlineData("hello", true)]
    [InlineData("", true)]
    [InlineData(null, false)]
    [InlineData(123, false)]
    public void Validate_BasicStringValidation_ReturnsExpectedResult(object? value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.String();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("hello", 3, true)]
    [InlineData("hi", 3, false)]
    [InlineData("", 1, false)]
    public void MinLength_ValidatesMinimumLength_ReturnsExpectedResult(string value, int minLength, bool expectedValid)
    {
        // Arrange
        var validator = Schema.String().MinLength(minLength);

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("hello", 10, true)]
    [InlineData("hello world", 5, false)]
    public void MaxLength_ValidatesMaximumLength_ReturnsExpectedResult(string value, int maxLength, bool expectedValid)
    {
        // Arrange
        var validator = Schema.String().MaxLength(maxLength);

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void Email_ValidatesEmailFormat_ReturnsExpectedResult(string value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.String().Email();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void WithMessage_CustomErrorMessage_UsesCustomMessage()
    {
        // Arrange
        var customMessage = "Custom error message";
        var validator = Schema.String().MinLength(5).WithMessage(customMessage);

        // Act
        var result = validator.Validate("hi");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(customMessage);
    }

    [Fact]
    public void Optional_WithNullValue_ReturnsValid()
    {
        // Arrange
        var validator = Schema.String().MinLength(5).Optional();

        // Act
        var result = validator.Validate(null);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class NumberValidatorTests
{
    [Theory]
    [InlineData(42, true)]
    [InlineData(42.5, true)]
    [InlineData("42", true)]
    [InlineData("not a number", false)]
    [InlineData(null, false)]
    public void Validate_BasicNumberValidation_ReturnsExpectedResult(object? value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Number();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(3, 5, false)]
    [InlineData(5, 5, true)]
    public void Min_ValidatesMinimumValue_ReturnsExpectedResult(decimal value, decimal min, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Number().Min(min);

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(5, 10, true)]
    [InlineData(15, 10, false)]
    [InlineData(10, 10, true)]
    public void Max_ValidatesMaximumValue_ReturnsExpectedResult(decimal value, decimal max, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Number().Max(max);

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(-5, false)]
    [InlineData(0, false)]
    public void Positive_ValidatesPositiveNumber_ReturnsExpectedResult(decimal value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Number().Positive();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(5.5, false)]
    [InlineData(-5, true)]
    public void Integer_ValidatesInteger_ReturnsExpectedResult(decimal value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Number().Integer();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}

public class BooleanValidatorTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData("true", true)]
    [InlineData("false", true)]
    [InlineData("not a boolean", false)]
    [InlineData(null, false)]
    public void Validate_BasicBooleanValidation_ReturnsExpectedResult(object? value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Boolean();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsTrue_ValidatesTrueValue_ReturnsExpectedResult(bool value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Boolean().IsTrue();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void IsFalse_ValidatesFalseValue_ReturnsExpectedResult(bool value, bool expectedValid)
    {
        // Arrange
        var validator = Schema.Boolean().IsFalse();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}

public class DateValidatorTests
{
    [Fact]
    public void Validate_ValidDate_ReturnsValid()
    {
        // Arrange
        var validator = Schema.Date();
        var date = new DateTime(2023, 1, 1);

        // Act
        var result = validator.Validate(date);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidValue_ReturnsInvalid()
    {
        // Arrange
        var validator = Schema.Date();

        // Act
        var result = validator.Validate("not a date");

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void After_ValidatesDateAfter_ReturnsExpectedResult()
    {
        // Arrange
        var minDate = new DateTime(2023, 1, 1);
        var validator = Schema.Date().After(minDate);

        // Act
        var validResult = validator.Validate(new DateTime(2023, 2, 1));
        var invalidResult = validator.Validate(new DateTime(2022, 12, 1));

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Before_ValidatesDateBefore_ReturnsExpectedResult()
    {
        // Arrange
        var maxDate = new DateTime(2023, 12, 31);
        var validator = Schema.Date().Before(maxDate);

        // Act
        var validResult = validator.Validate(new DateTime(2023, 6, 1));
        var invalidResult = validator.Validate(new DateTime(2024, 1, 1));

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
    }
} 