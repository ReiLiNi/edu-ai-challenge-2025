using FluentAssertions;
using Xunit;

namespace ValidationFramework.Tests;

public class ArrayValidatorTests
{
    [Fact]
    public void Validate_ValidStringArray_ReturnsValid()
    {
        // Arrange
        var validator = Schema.Array(Schema.String());
        var array = new[] { "hello", "world" };

        // Act
        var result = validator.Validate(array);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ArrayWithInvalidItems_ReturnsInvalid()
    {
        // Arrange
        var validator = Schema.Array(Schema.String().MinLength(5));
        var array = new[] { "hello", "hi" }; // "hi" is too short

        // Act
        var result = validator.Validate(array);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Item 1"));
    }

    [Fact]
    public void MinLength_ValidatesArrayMinLength_ReturnsExpectedResult()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).MinLength(3);

        // Act
        var validResult = validator.Validate(new[] { "a", "b", "c" });
        var invalidResult = validator.Validate(new[] { "a", "b" });

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void MaxLength_ValidatesArrayMaxLength_ReturnsExpectedResult()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).MaxLength(2);

        // Act
        var validResult = validator.Validate(new[] { "a", "b" });
        var invalidResult = validator.Validate(new[] { "a", "b", "c" });

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void NotEmpty_ValidatesNonEmptyArray_ReturnsExpectedResult()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).NotEmpty();

        // Act
        var validResult = validator.Validate(new[] { "a" });
        var invalidResult = validator.Validate(new string[0]);

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NullArray_ReturnsInvalid()
    {
        // Arrange
        var validator = Schema.Array(Schema.String());

        // Act
        var result = validator.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Array cannot be null");
    }
}

public class ObjectValidatorTests
{
    public class TestUser
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    [Fact]
    public void Validate_ValidObject_ReturnsValid()
    {
        // Arrange
        var schema = new Dictionary<string, IValidator<object>>
        {
            { "Name", Schema.String().MinLength(2).AsBase() },
            { "Email", Schema.String().Email().AsBase() },
            { "Age", Schema.Number().Min(0).AsBase() },
            { "IsActive", Schema.Boolean().AsBase() }
        };
        var validator = Schema.Object<TestUser>(schema);

        var user = new TestUser
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true
        };

        // Act
        var result = validator.Validate(user);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidObject_ReturnsInvalid()
    {
        // Arrange
        var schema = new Dictionary<string, IValidator<object>>
        {
            { "Name", Schema.String().MinLength(5).AsBase() },
            { "Email", Schema.String().Email().AsBase() }
        };
        var validator = Schema.Object<TestUser>(schema);

        var user = new TestUser
        {
            Name = "Jo", // Too short
            Email = "invalid-email" // Invalid format
        };

        // Act
        var result = validator.Validate(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Contains("Name"));
        result.Errors.Should().Contain(e => e.Contains("Email"));
    }

    [Fact]
    public void Validate_DynamicObject_ReturnsValid()
    {
        // Arrange
        var schema = new Dictionary<string, IValidator<object>>
        {
            { "name", Schema.String().MinLength(2).AsBase() },
            { "age", Schema.Number().Min(0).AsBase() }
        };
        var validator = Schema.Object(schema);

        var obj = new Dictionary<string, object>
        {
            { "name", "John" },
            { "age", 30 }
        };

        // Act
        var result = validator.Validate(obj);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullObject_ReturnsInvalid()
    {
        // Arrange
        var schema = new Dictionary<string, IValidator<object>>
        {
            { "name", Schema.String().AsBase() }
        };
        var validator = Schema.Object(schema);

        // Act
        var result = validator.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Object cannot be null");
    }
}

public class SchemaTests
{
    [Fact]
    public void Optional_WithValidValue_ReturnsValid()
    {
        // Arrange
        var validator = Schema.String().MinLength(5).Optional();

        // Act
        var result = validator.Validate("hello world");

        // Assert
        result.IsValid.Should().BeTrue();
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

    [Fact]
    public void Any_WithAnyValue_ReturnsValid()
    {
        // Arrange
        var validator = Schema.Any();

        // Act
        var stringResult = validator.Validate("test");
        var numberResult = validator.Validate(42);
        var nullResult = validator.Validate(null);

        // Assert
        stringResult.IsValid.Should().BeTrue();
        numberResult.IsValid.Should().BeTrue();
        nullResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public void OneOf_WithMatchingValidator_ReturnsValid()
    {
        // Arrange
        var validator = Schema.OneOf(
            Schema.String().AsBase(),
            Schema.Number().AsBase()
        );

        // Act
        var stringResult = validator.Validate("test");
        var numberResult = validator.Validate(42);
        var invalidResult = validator.Validate(true);

        // Assert
        stringResult.IsValid.Should().BeTrue();
        numberResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
    }
} 