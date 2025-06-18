using System.Collections.Immutable;

namespace ValidationFramework.Core;

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public record ValidationResult
{
    private ValidationResult(bool isValid, ImmutableList<ValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }
    
    /// <summary>
    /// Indicates whether the validation was successful
    /// </summary>
    public bool IsValid { get; }
    
    /// <summary>
    /// Collection of validation errors (empty if validation succeeded)
    /// </summary>
    public ImmutableList<ValidationError> Errors { get; }
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new(true, ImmutableList<ValidationError>.Empty);
    
    /// <summary>
    /// Creates a failed validation result with a single error
    /// </summary>
    /// <param name="error">The validation error</param>
    public static ValidationResult Failure(ValidationError error) => new(false, ImmutableList.Create(error));
    
    /// <summary>
    /// Creates a failed validation result with multiple errors
    /// </summary>
    /// <param name="errors">The validation errors</param>
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => 
        new(false, errors.ToImmutableList());
    
    /// <summary>
    /// Creates a failed validation result with a simple message
    /// </summary>
    /// <param name="path">The path to the invalid value</param>
    /// <param name="message">The error message</param>
    public static ValidationResult Failure(string path, string message) => 
        Failure(new ValidationError(path, message));
    
    /// <summary>
    /// Combines multiple validation results
    /// </summary>
    /// <param name="results">The results to combine</param>
    public static ValidationResult Combine(params ValidationResult[] results) => 
        Combine((IEnumerable<ValidationResult>)results);
    
    /// <summary>
    /// Combines multiple validation results
    /// </summary>
    /// <param name="results">The results to combine</param>
    public static ValidationResult Combine(IEnumerable<ValidationResult> results)
    {
        var allErrors = results.SelectMany(r => r.Errors).ToImmutableList();
        return allErrors.IsEmpty ? Success() : new ValidationResult(false, allErrors);
    }
    
    /// <summary>
    /// Converts this result to a string representation
    /// </summary>
    public override string ToString()
    {
        if (IsValid) return "Validation succeeded";
        
        var errorMessages = Errors.Select(e => $"{e.Path}: {e.Message}");
        return $"Validation failed:\n{string.Join("\n", errorMessages)}";
    }
}

/// <summary>
/// Represents a single validation error
/// </summary>
/// <param name="Path">The path to the field that failed validation</param>
/// <param name="Message">The error message describing the failure</param>
public record ValidationError(string Path, string Message)
{
    /// <summary>
    /// Creates a validation error with an empty path
    /// </summary>
    /// <param name="message">The error message</param>
    public ValidationError(string message) : this("", message) { }
} 