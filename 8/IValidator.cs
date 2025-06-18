using System.Diagnostics.CodeAnalysis;

namespace ValidationFramework.Core;

/// <summary>
/// Core interface for all validators in the framework
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public interface IValidator<T>
{
    /// <summary>
    /// Validates a value and returns a validation result
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="path">The path to the current value (for nested validation)</param>
    /// <returns>A validation result indicating success or failure</returns>
    ValidationResult Validate(T? value, string path = "");
    
    /// <summary>
    /// Attempts to validate a value and returns true if valid
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="validatedValue">The validated value if successful</param>
    /// <returns>True if validation succeeds, false otherwise</returns>
    bool TryValidate(T? value, [NotNullWhen(true)] out T? validatedValue);
}

/// <summary>
/// Base interface for fluent validator configuration
/// </summary>
/// <typeparam name="TValidator">The validator type for fluent chaining</typeparam>
/// <typeparam name="TValue">The value type being validated</typeparam>
public interface IFluentValidator<out TValidator, TValue> : IValidator<TValue>
    where TValidator : IValidator<TValue>
{
    /// <summary>
    /// Marks this field as optional (nullable)
    /// </summary>
    /// <returns>An optional validator wrapper</returns>
    OptionalValidator<TValue> Optional();
    
    /// <summary>
    /// Sets a custom error message for validation failures
    /// </summary>
    /// <param name="message">The custom error message</param>
    /// <returns>The validator with custom message</returns>
    TValidator WithMessage(string message);
} 