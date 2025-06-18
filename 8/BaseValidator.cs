using System.Diagnostics.CodeAnalysis;

namespace ValidationFramework.Core;

/// <summary>
/// Base class for all validators providing common functionality
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public abstract class BaseValidator<T> : IValidator<T>
{
    protected string? CustomMessage { get; private set; }
    
    /// <summary>
    /// Sets a custom error message for this validator
    /// </summary>
    /// <param name="message">The custom error message</param>
    protected void SetCustomMessage(string message)
    {
        CustomMessage = message;
    }
    
    /// <summary>
    /// Validates a value and returns a validation result
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="path">The path to the current value</param>
    /// <returns>A validation result</returns>
    public abstract ValidationResult Validate(T? value, string path = "");
    
    /// <summary>
    /// Attempts to validate a value
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="validatedValue">The validated value if successful</param>
    /// <returns>True if validation succeeds</returns>
    public virtual bool TryValidate(T? value, [NotNullWhen(true)] out T? validatedValue)
    {
        var result = Validate(value);
        if (result.IsValid)
        {
            validatedValue = value;
            return true;
        }
        
        validatedValue = default;
        return false;
    }
    
    /// <summary>
    /// Creates an error message using either the custom message or default message
    /// </summary>
    /// <param name="defaultMessage">The default error message</param>
    /// <returns>The error message to use</returns>
    protected string GetErrorMessage(string defaultMessage) => CustomMessage ?? defaultMessage;
    
    /// <summary>
    /// Helper method to create a successful validation result
    /// </summary>
    protected static ValidationResult Success() => ValidationResult.Success();
    
    /// <summary>
    /// Helper method to create a failed validation result
    /// </summary>
    /// <param name="path">The path to the invalid value</param>
    /// <param name="message">The error message</param>
    protected static ValidationResult Failure(string path, string message) => 
        ValidationResult.Failure(path, message);
}

/// <summary>
/// Wrapper that makes any validator optional (nullable)
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public class OptionalValidator<T> : IValidator<T?>
{
    private readonly IValidator<T> _innerValidator;
    
    public OptionalValidator(IValidator<T> innerValidator)
    {
        _innerValidator = innerValidator ?? throw new ArgumentNullException(nameof(innerValidator));
    }
    
    public ValidationResult Validate(T? value, string path = "")
    {
        // Null values are always valid for optional validators
        if (value is null)
            return ValidationResult.Success();
        
        return _innerValidator.Validate(value, path);
    }
    
    public bool TryValidate(T? value, [NotNullWhen(true)] out T? validatedValue)
    {
        if (value is null)
        {
            validatedValue = default;
            return true;
        }
        
        return _innerValidator.TryValidate(value, out validatedValue);
    }
} 