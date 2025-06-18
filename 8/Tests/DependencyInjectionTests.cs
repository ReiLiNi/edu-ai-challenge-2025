using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using ValidationFramework.Core;
using ValidationFramework.DependencyInjection;
using ValidationFramework.Examples;
using ValidationFramework.Validators;
using Xunit;

namespace ValidationFramework.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddValidationFramework_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddValidationFramework();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IValidatorFactory>().Should().NotBeNull();
        serviceProvider.GetService<IValidationService>().Should().NotBeNull();
        serviceProvider.GetService<IOptions<ValidationFrameworkOptions>>().Should().NotBeNull();
    }

    [Fact]
    public void AddValidationFramework_WithOptions_ShouldConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddValidationFramework(options =>
        {
            options.StopOnFirstError = true;
            options.IncludePropertyPaths = false;
            options.DefaultErrorMessageFormat = "Custom: {0}";
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<ValidationFrameworkOptions>>().Value;
        options.StopOnFirstError.Should().BeTrue();
        options.IncludePropertyPaths.Should().BeFalse();
        options.DefaultErrorMessageFormat.Should().Be("Custom: {0}");
    }

    [Fact]
    public void AddValidator_ShouldRegisterValidatorForType()
    {
        // Arrange
        var services = new ServiceCollection();
        var userValidator = ExampleValidators.CreateUserValidator();

        // Act
        services.AddValidationFramework();
        services.AddValidator<User>(userValidator);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var factory = serviceProvider.GetRequiredService<IValidatorFactory>();
        var retrievedValidator = factory.GetValidator<User>();
        retrievedValidator.Should().NotBeNull();
        retrievedValidator.Should().BeSameAs(userValidator);
    }

    [Fact]
    public void ValidatorFactory_GetValidator_WithRegisteredType_ShouldReturnValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        var stringValidator = new StringValidator().MinLength(3);

        services.AddValidationFramework();
        services.AddValidator<string>(stringValidator);
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IValidatorFactory>();

        // Act
        var result = factory.GetValidator<string>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(stringValidator);
    }

    [Fact]
    public void ValidatorFactory_GetValidator_WithUnregisteredType_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValidationFramework();
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IValidatorFactory>();

        // Act & Assert
        var act = () => factory.GetValidator<int>();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*validator*registered*int*");
    }

    [Fact]
    public void ValidatorFactory_HasValidator_WithRegisteredType_ShouldReturnTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        var stringValidator = new StringValidator();

        services.AddValidationFramework();
        services.AddValidator<string>(stringValidator);
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IValidatorFactory>();

        // Act
        var result = factory.HasValidator<string>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidatorFactory_HasValidator_WithUnregisteredType_ShouldReturnFalse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValidationFramework();
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IValidatorFactory>();

        // Act
        var result = factory.HasValidator<int>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidationService_Validate_WithRegisteredValidator_ShouldUseValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        var stringValidator = new StringValidator().MinLength(5);
        
        services.AddValidationFramework();
        services.AddValidator<string>(stringValidator);
        var serviceProvider = services.BuildServiceProvider();
        var validationService = serviceProvider.GetRequiredService<IValidationService>();

        // Act
        var validResult = validationService.Validate("Hello World");
        var invalidResult = validationService.Validate("Hi");

        // Assert
        validResult.IsValid.Should().BeTrue();
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void ValidationService_Validate_WithUnregisteredValidator_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValidationFramework();
        var serviceProvider = services.BuildServiceProvider();
        var validationService = serviceProvider.GetRequiredService<IValidationService>();

        // Act & Assert
        var act = () => validationService.Validate(42);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValidationService_TryValidate_WithValidValue_ShouldReturnTrueAndValue()
    {
        // Arrange
        var services = new ServiceCollection();
        var stringValidator = new StringValidator().MinLength(3);
        
        services.AddValidationFramework();
        services.AddValidator<string>(stringValidator);
        var serviceProvider = services.BuildServiceProvider();
        var validationService = serviceProvider.GetRequiredService<IValidationService>();

        // Act
        var result = validationService.TryValidate("Hello", out var validatedValue);

        // Assert
        result.Should().BeTrue();
        validatedValue.Should().Be("Hello");
    }

    [Fact]
    public void ValidationService_TryValidate_WithInvalidValue_ShouldReturnFalseAndDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        var stringValidator = new StringValidator().MinLength(10);
        
        services.AddValidationFramework();
        services.AddValidator<string>(stringValidator);
        var serviceProvider = services.BuildServiceProvider();
        var validationService = serviceProvider.GetRequiredService<IValidationService>();

        // Act
        var result = validationService.TryValidate("Hi", out var validatedValue);

        // Assert
        result.Should().BeFalse();
        validatedValue.Should().BeNull();
    }

    [Fact]
    public void ValidationService_WithStopOnFirstErrorOption_ShouldRespectConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockValidator = new Mock<IValidator<string>>();
        
        // Setup mock to return multiple errors
        var multipleErrors = ValidationResult.Failure(new[]
        {
            new ValidationError("field1", "Error 1"),
            new ValidationError("field2", "Error 2")
        });
        mockValidator.Setup(v => v.Validate(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(multipleErrors);

        services.AddValidationFramework(options => options.StopOnFirstError = true);
        services.AddValidator<string>(mockValidator.Object);
        var serviceProvider = services.BuildServiceProvider();
        var validationService = serviceProvider.GetRequiredService<IValidationService>();

        // Act
        var result = validationService.Validate("test");

        // Assert
        result.IsValid.Should().BeFalse();
        // The actual behavior depends on implementation - this test documents expected behavior
        mockValidator.Verify(v => v.Validate("test", ""), Times.Once);
    }

    [Fact]
    public void ServiceCollection_Extensions_ShouldBeChainable()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services
            .AddValidationFramework(options => options.StopOnFirstError = true)
            .AddValidator<string>(new StringValidator().MinLength(3))
            .AddValidator<int?>(new IntegerValidator().Min(0));

        // Assert
        result.Should().BeSameAs(services);
        
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IValidatorFactory>();
        
        factory.HasValidator<string>().Should().BeTrue();
        factory.HasValidator<int?>().Should().BeTrue();
    }

    [Fact]
    public void ValidationFrameworkOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ValidationFrameworkOptions();

        // Assert
        options.StopOnFirstError.Should().BeFalse();
        options.IncludePropertyPaths.Should().BeTrue();
        options.DefaultErrorMessageFormat.Should().Be("{0}");
    }
} 