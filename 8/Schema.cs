using ValidationFramework.Core;
using ValidationFramework.Validators;

namespace ValidationFramework;

/// <summary>
/// Main entry point for creating validators with a fluent API
/// </summary>
public static class Schema
{
    /// <summary>
    /// Creates a string validator
    /// </summary>
    /// <returns>A new string validator</returns>
    public static StringValidator String() => new();
    
    /// <summary>
    /// Creates a number (double) validator
    /// </summary>
    /// <returns>A new number validator</returns>
    public static NumberValidator Number() => new();
    
    /// <summary>
    /// Creates an integer validator
    /// </summary>
    /// <returns>A new integer validator</returns>
    public static IntegerValidator Integer() => new();
    
    /// <summary>
    /// Creates a boolean validator
    /// </summary>
    /// <returns>A new boolean validator</returns>
    public static BooleanValidator Boolean() => new();
    
    /// <summary>
    /// Creates a date validator
    /// </summary>
    /// <returns>A new date validator</returns>
    public static DateValidator Date() => new();
    
    /// <summary>
    /// Creates an array validator with the specified item validator
    /// </summary>
    /// <typeparam name="T">The type of items in the array</typeparam>
    /// <param name="itemValidator">The validator for array items</param>
    /// <returns>A new array validator</returns>
    public static ArrayValidator<T> Array<T>(IValidator<T> itemValidator) => new(itemValidator);
    
    /// <summary>
    /// Creates an object validator for the specified type
    /// </summary>
    /// <typeparam name="T">The type of object to validate</typeparam>
    /// <returns>A new object validator</returns>
    public static ObjectValidator<T> Object<T>() where T : class => new();
    
    /// <summary>
    /// Creates an object validator with predefined property validators
    /// </summary>
    /// <typeparam name="T">The type of object to validate</typeparam>
    /// <param name="propertyValidators">Dictionary of property validators</param>
    /// <returns>A new object validator</returns>
    public static ObjectValidator<T> Object<T>(Dictionary<string, IValidator<object?>> propertyValidators) 
        where T : class => new(propertyValidators);
    
    /// <summary>
    /// Creates a custom validator from a predicate function
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="predicate">The validation predicate</param>
    /// <param name="errorMessage">Error message when validation fails</param>
    /// <returns>A new custom validator</returns>
    public static CustomValidator<T> Custom<T>(Func<T?, bool> predicate, string errorMessage = "Validation failed") =>
        new(predicate, errorMessage);
    
    /// <summary>
    /// Creates a custom validator with detailed validation logic
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="validationFunc">The validation function</param>
    /// <returns>A new custom validator</returns>
    public static CustomValidator<T> Custom<T>(Func<T?, string, ValidationResult> validationFunc) =>
        new(validationFunc);
}

/// <summary>
/// Custom validator that allows user-defined validation logic
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public class CustomValidator<T> : BaseValidator<T>, IFluentValidator<CustomValidator<T>, T>
{
    private readonly Func<T?, string, ValidationResult> _validationFunc;
    
    public CustomValidator(Func<T?, bool> predicate, string errorMessage)
    {
        _validationFunc = (value, path) => predicate(value) 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(path, errorMessage);
    }
    
    public CustomValidator(Func<T?, string, ValidationResult> validationFunc)
    {
        _validationFunc = validationFunc ?? throw new ArgumentNullException(nameof(validationFunc));
    }
    
    public CustomValidator<T> WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<T> Optional() => new(this);
    
    public override ValidationResult Validate(T? value, string path = "")
    {
        var result = _validationFunc(value, path);
        
        // If there's a custom message and the validation failed, override the message
        if (!result.IsValid && CustomMessage is not null)
        {
            var errors = result.Errors.Select(e => new ValidationError(e.Path, CustomMessage));
            return ValidationResult.Failure(errors);
        }
        
        return result;
    }
} 