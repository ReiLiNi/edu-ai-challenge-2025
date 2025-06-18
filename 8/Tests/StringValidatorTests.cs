using AutoFixture;
using FluentAssertions;
using ValidationFramework.Core;
using ValidationFramework.Validators;
using Xunit;

namespace ValidationFramework.Tests;

public class StringValidatorTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Validate_WithValidString_ShouldReturnSuccess()
    {
        // Arrange
        var validator = new StringValidator().MinLength(3).MaxLength(10);
        var validString = "Hello";

        // Act
        var result = validator.Validate(validString);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullString_ShouldReturnFailure()
    {
        // Arrange
        var validator = new StringValidator();

        // Act
        var result = validator.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain("cannot be null");
    }

    [Theory]
    [InlineData("ab", 3)] // Too short
    [InlineData("a", 3)]  // Too short
    [InlineData("", 1)]   // Empty string with min length
    public void Validate_WithStringTooShort_ShouldReturnFailure(string input, int minLength)
    {
        // Arrange
        var validator = new StringValidator().MinLength(minLength);

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain($"at least {minLength} characters");
    }

    [Theory]
    [InlineData("12345678901", 10)] // Too long
    [InlineData("123456", 5)]       // Too long
    public void Validate_WithStringTooLong_ShouldReturnFailure(string input, int maxLength)
    {
        // Arrange
        var validator = new StringValidator().MaxLength(maxLength);

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain($"at most {maxLength} characters");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("simple@test.org")]
    public void Validate_WithValidEmail_ShouldReturnSuccess(string email)
    {
        // Arrange
        var validator = new StringValidator().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");

        // Act
        var result = validator.Validate(email);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    public void Validate_WithInvalidEmail_ShouldReturnFailure(string email)
    {
        // Arrange
        var validator = new StringValidator().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");

        // Act
        var result = validator.Validate(email);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain("does not match the required pattern");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n\r")]
    public void Validate_WithEmptyOrWhitespaceString_WhenNotEmptyRequired_ShouldReturnFailure(string input)
    {
        // Arrange
        var validator = new StringValidator().NotEmpty();

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain("cannot be empty");
    }

    [Theory]
    [InlineData("Héllo Wörld")]
    [InlineData("こんにちは")]
    [InlineData("🌟⭐✨")]
    [InlineData("Москва")]
    [InlineData("العربية")]
    public void Validate_WithUnicodeCharacters_ShouldReturnSuccess(string input)
    {
        // Arrange
        var validator = new StringValidator().MinLength(1).MaxLength(50);

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithChainedValidators_ShouldApplyAllRules()
    {
        // Arrange
        var validator = new StringValidator()
            .NotEmpty()
            .MinLength(8)
            .MaxLength(50)
            .Pattern(@"^[A-Za-z0-9@._-]+$");

        // Act
        var validResult = validator.Validate("user123@test.com");
        var invalidResult = validator.Validate("usr");

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Errors.Should().HaveCount(1);
        invalidResult.Errors[0].Message.Should().Contain("at least 8 characters");
    }

    [Fact]
    public void Validate_WithCustomMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var customMessage = "Please provide a valid username";
        var validator = new StringValidator()
            .MinLength(3)
            .WithMessage(customMessage);

        // Act
        var result = validator.Validate("ab");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Be(customMessage);
    }

    [Fact]
    public void Optional_WithNullValue_ShouldReturnSuccess()
    {
        // Arrange
        var validator = new StringValidator().MinLength(3).Optional();

        // Act
        var result = validator.Validate(null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Optional_WithValidValue_ShouldValidateNormally()
    {
        // Arrange
        var validator = new StringValidator().MinLength(3).Optional();

        // Act
        var validResult = validator.Validate("Hello");
        var invalidResult = validator.Validate("Hi");

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidate_WithValidString_ShouldReturnTrueAndValue()
    {
        // Arrange
        var validator = new StringValidator().MinLength(3);
        var input = "Hello";

        // Act
        var success = validator.TryValidate(input, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(input);
    }

    [Fact]
    public void TryValidate_WithInvalidString_ShouldReturnFalseAndDefault()
    {
        // Arrange
        var validator = new StringValidator().MinLength(5);
        var input = "Hi";

        // Act
        var success = validator.TryValidate(input, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(5, 5, "12345")] // Exact length
    [InlineData(1, 1, "a")]     // Single character
    [InlineData(0, 0, "")]      // Empty string allowed
    public void Validate_WithBoundaryValues_ShouldReturnSuccess(int minLength, int maxLength, string input)
    {
        // Arrange
        var validator = new StringValidator().MinLength(minLength).MaxLength(maxLength);

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMultipleViolations_ShouldReturnFirstError()
    {
        // Arrange
        var validator = new StringValidator()
            .NotEmpty()
            .MinLength(10)
            .Pattern(@"^\d+$"); // Numbers only

        // Act
        var result = validator.Validate(""); // Violates NotEmpty, MinLength, and Pattern

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1); // Should stop at first error
        result.Errors[0].Message.Should().Contain("cannot be empty");
    }
} 