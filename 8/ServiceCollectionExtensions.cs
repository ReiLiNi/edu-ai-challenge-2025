using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ValidationFramework.Core;

namespace ValidationFramework.DependencyInjection;

/// <summary>
/// Extension methods for registering validation services with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds validation framework services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for fluent configuration</returns>
    public static IServiceCollection AddValidationFramework(this IServiceCollection services)
    {
        services.TryAddSingleton<IValidatorFactory, ValidatorFactory>();
        services.TryAddTransient<IValidationService, ValidationService>();
        
        return services;
    }
    
    /// <summary>
    /// Adds validation framework services with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>The service collection for fluent configuration</returns>
    public static IServiceCollection AddValidationFramework(
        this IServiceCollection services, 
        Action<ValidationFrameworkOptions> configure)
    {
        services.Configure(configure);
        return services.AddValidationFramework();
    }
    
    /// <summary>
    /// Registers a validator for a specific type
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="validator">The validator instance</param>
    /// <returns>The service collection for fluent configuration</returns>
    public static IServiceCollection AddValidator<T>(this IServiceCollection services, IValidator<T> validator)
    {
        services.AddSingleton(validator);
        return services;
    }
    
    /// <summary>
    /// Registers a validator factory for a specific type
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="validatorFactory">Factory function to create the validator</param>
    /// <returns>The service collection for fluent configuration</returns>
    public static IServiceCollection AddValidator<T>(
        this IServiceCollection services, 
        Func<IServiceProvider, IValidator<T>> validatorFactory)
    {
        services.AddSingleton(validatorFactory);
        return services;
    }
}

/// <summary>
/// Configuration options for the validation framework
/// </summary>
public class ValidationFrameworkOptions
{
    /// <summary>
    /// Whether to stop validation on the first error (default: false)
    /// </summary>
    public bool StopOnFirstError { get; set; } = false;
    
    /// <summary>
    /// Default error message format
    /// </summary>
    public string DefaultErrorMessageFormat { get; set; } = "{0}";
    
    /// <summary>
    /// Whether to include property paths in error messages (default: true)
    /// </summary>
    public bool IncludePropertyPaths { get; set; } = true;
}

/// <summary>
/// Factory interface for creating validators
/// </summary>
public interface IValidatorFactory
{
    /// <summary>
    /// Creates a validator for the specified type
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <returns>A validator for the type, or null if none is registered</returns>
    IValidator<T>? GetValidator<T>();
    
    /// <summary>
    /// Checks if a validator is registered for the specified type
    /// </summary>
    /// <typeparam name="T">The type to check</typeparam>
    /// <returns>True if a validator is registered, false otherwise</returns>
    bool HasValidator<T>();
}

/// <summary>
/// Default implementation of the validator factory
/// </summary>
public class ValidatorFactory : IValidatorFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ValidatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    public IValidator<T>? GetValidator<T>()
    {
        return _serviceProvider.GetService<IValidator<T>>();
    }
    
    public bool HasValidator<T>()
    {
        return _serviceProvider.GetService<IValidator<T>>() is not null;
    }
}

/// <summary>
/// Service interface for validation operations
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a value using a registered validator
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="value">The value to validate</param>
    /// <returns>The validation result</returns>
    ValidationResult Validate<T>(T? value);
    
    /// <summary>
    /// Validates a value using a specific validator
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="validator">The validator to use</param>
    /// <returns>The validation result</returns>
    ValidationResult Validate<T>(T? value, IValidator<T> validator);
    
    /// <summary>
    /// Attempts to validate a value using a registered validator
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="value">The value to validate</param>
    /// <param name="validatedValue">The validated value if successful</param>
    /// <returns>True if validation succeeds, false otherwise</returns>
    bool TryValidate<T>(T? value, out T? validatedValue);
}

/// <summary>
/// Default implementation of the validation service
/// </summary>
public class ValidationService : IValidationService
{
    private readonly IValidatorFactory _validatorFactory;
    
    public ValidationService(IValidatorFactory validatorFactory)
    {
        _validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
    }
    
    public ValidationResult Validate<T>(T? value)
    {
        var validator = _validatorFactory.GetValidator<T>();
        if (validator is null)
        {
            throw new InvalidOperationException($"No validator registered for type {typeof(T).Name}");
        }
        
        return validator.Validate(value);
    }
    
    public ValidationResult Validate<T>(T? value, IValidator<T> validator)
    {
        return validator.Validate(value);
    }
    
    public bool TryValidate<T>(T? value, out T? validatedValue)
    {
        var validator = _validatorFactory.GetValidator<T>();
        if (validator is null)
        {
            validatedValue = default;
            return false;
        }
        
        return validator.TryValidate(value, out validatedValue);
    }
} 