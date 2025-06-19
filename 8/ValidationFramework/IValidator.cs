namespace ValidationFramework;

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public object? Value { get; init; }

    public static ValidationResult Success(object? value = null) => new() { IsValid = true, Value = value };
    public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
    public static ValidationResult Failure(List<string> errors) => new() { IsValid = false, Errors = errors };
}

/// <summary>
/// Base interface for all validators
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public interface IValidator<T>
{
    ValidationResult Validate(object? value);
    IValidator<T> WithMessage(string message);
    IValidator<T?> Optional();
    IValidator<object> AsBase();
} 