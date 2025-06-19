using System.Text.RegularExpressions;

namespace ValidationFramework;

/// <summary>
/// Main entry point for creating validators with fluent API
/// </summary>
public static class Schema
{
    /// <summary>
    /// Creates a string validator
    /// </summary>
    /// <returns>A new StringValidator instance</returns>
    public static StringValidator String() => new();

    /// <summary>
    /// Creates a number validator
    /// </summary>
    /// <returns>A new NumberValidator instance</returns>
    public static NumberValidator Number() => new();

    /// <summary>
    /// Creates a boolean validator
    /// </summary>
    /// <returns>A new BooleanValidator instance</returns>
    public static BooleanValidator Boolean() => new();

    /// <summary>
    /// Creates a date validator
    /// </summary>
    /// <returns>A new DateValidator instance</returns>
    public static DateValidator Date() => new();

    /// <summary>
    /// Creates an array validator for the specified item type
    /// </summary>
    /// <typeparam name="T">The type of items in the array</typeparam>
    /// <param name="itemValidator">Validator for individual array items</param>
    /// <returns>A new ArrayValidator instance</returns>
    public static ArrayValidator<T> Array<T>(IValidator<T> itemValidator) => new(itemValidator);

    /// <summary>
    /// Creates an object validator with a defined schema
    /// </summary>
    /// <typeparam name="T">The type of object to validate</typeparam>
    /// <param name="schema">Dictionary defining the validation rules for each property</param>
    /// <returns>A new ObjectValidator instance</returns>
    public static ObjectValidator<T> Object<T>(Dictionary<string, IValidator<object>> schema) where T : class 
        => new(schema);

    /// <summary>
    /// Creates a dynamic object validator that works with dictionaries and anonymous objects
    /// </summary>
    /// <param name="schema">Dictionary defining the validation rules for each property</param>
    /// <returns>A new DynamicObjectValidator instance</returns>
    public static DynamicObjectValidator Object(Dictionary<string, IValidator<object>> schema) 
        => new(schema);

    /// <summary>
    /// Helper method to create regex patterns
    /// </summary>
    /// <param name="pattern">The regex pattern string</param>
    /// <param name="options">Regex options (default: None)</param>
    /// <returns>A compiled Regex instance</returns>
    public static Regex Pattern(string pattern, RegexOptions options = RegexOptions.None) 
        => new(pattern, options);

    /// <summary>
    /// Creates a validator that accepts any value (always passes validation)
    /// </summary>
    /// <returns>A validator that always succeeds</returns>
    public static IValidator<object> Any() => new AnyValidator();

    /// <summary>
    /// Creates a validator for nullable types
    /// </summary>
    /// <typeparam name="T">The underlying type</typeparam>
    /// <param name="validator">Validator for non-null values</param>
    /// <returns>A validator that allows null values</returns>
    public static IValidator<T?> Nullable<T>(IValidator<T> validator) where T : struct
        => new NullableValidator<T>(validator);

    /// <summary>
    /// Creates a validator that validates against multiple possible validators (union type)
    /// </summary>
    /// <param name="validators">Array of validators to try</param>
    /// <returns>A validator that succeeds if any of the provided validators succeed</returns>
    public static IValidator<object> OneOf(params IValidator<object>[] validators)
        => new OneOfValidator(validators);
}

/// <summary>
/// Validator that accepts any value
/// </summary>
internal class AnyValidator : BaseValidator<object>
{
    public override ValidationResult Validate(object? value)
    {
        return ValidationResult.Success(value);
    }
}

/// <summary>
/// Validator for nullable value types
/// </summary>
/// <typeparam name="T">The underlying value type</typeparam>
internal class NullableValidator<T> : BaseValidator<T?> where T : struct
{
    private readonly IValidator<T> _innerValidator;

    public NullableValidator(IValidator<T> innerValidator)
    {
        _innerValidator = innerValidator;
    }

    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Success(null);
        }

        return _innerValidator.Validate(value);
    }
}

/// <summary>
/// Validator that tries multiple validators and succeeds if any succeed
/// </summary>
internal class OneOfValidator : BaseValidator<object>
{
    private readonly IValidator<object>[] _validators;

    public OneOfValidator(IValidator<object>[] validators)
    {
        _validators = validators;
    }

    public override ValidationResult Validate(object? value)
    {
        var allErrors = new List<string>();

        foreach (var validator in _validators)
        {
            var result = validator.Validate(value);
            if (result.IsValid)
            {
                return result;
            }
            allErrors.AddRange(result.Errors);
        }

        return ValidationResult.Failure($"Value did not match any of the expected types. Errors: {string.Join("; ", allErrors)}");
    }
} 