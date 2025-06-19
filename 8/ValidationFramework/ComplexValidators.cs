using System.Collections;
using System.Reflection;

namespace ValidationFramework;

/// <summary>
/// Array validator for collections and lists
/// </summary>
/// <typeparam name="T">The type of items in the array</typeparam>
public class ArrayValidator<T> : BaseValidator<IEnumerable<T>>
{
    private readonly IValidator<T> _itemValidator;

    public ArrayValidator(IValidator<T> itemValidator)
    {
        _itemValidator = itemValidator;
    }

    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Failure(GetErrorMessage("Array cannot be null"));
        }

        if (value is not IEnumerable enumerable)
        {
            return ValidationResult.Failure(GetErrorMessage("Value must be an array or collection"));
        }

        var items = enumerable.Cast<object?>().ToList();
        var typedItems = new List<T>();
        var errors = new List<string>();

        // Validate each item
        for (int i = 0; i < items.Count; i++)
        {
            var itemResult = _itemValidator.Validate(items[i]);
            if (!itemResult.IsValid)
            {
                errors.AddRange(itemResult.Errors.Select(e => $"Item {i}: {e}"));
            }
            else if (itemResult.Value is T typedValue)
            {
                typedItems.Add(typedValue);
            }
        }

        if (errors.Count > 0)
        {
            return ValidationResult.Failure(errors);
        }

        // Apply array-level validations
        var arrayResult = ValidateInternal(typedItems);
        return arrayResult.IsValid 
            ? ValidationResult.Success(typedItems) 
            : arrayResult;
    }

    public ArrayValidator<T> MinLength(int minLength)
    {
        _validations.Add(value => value.Count() >= minLength 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"Array must have at least {minLength} items")));
        return this;
    }

    public ArrayValidator<T> MaxLength(int maxLength)
    {
        _validations.Add(value => value.Count() <= maxLength 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"Array must have at most {maxLength} items")));
        return this;
    }

    public ArrayValidator<T> NotEmpty()
    {
        _validations.Add(value => value.Any() 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage("Array cannot be empty")));
        return this;
    }
}

/// <summary>
/// Object validator for complex objects with defined schema
/// </summary>
/// <typeparam name="T">The type of object being validated</typeparam>
public class ObjectValidator<T> : BaseValidator<T> where T : class
{
    private readonly Dictionary<string, IValidator<object>> _schema = new();

    public ObjectValidator(Dictionary<string, IValidator<object>> schema)
    {
        _schema = schema;
    }

    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Failure(GetErrorMessage("Object cannot be null"));
        }

        var errors = new List<string>();
        var objectType = value.GetType();

        // Validate each property in the schema
        foreach (var (propertyName, validator) in _schema)
        {
            var property = objectType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                // Try to get the property using dictionary access (for dynamic objects)
                if (value is IDictionary<string, object> dict)
                {
                    var dictValue = dict.TryGetValue(propertyName, out var val) ? val : null;
                    var result = validator.Validate(dictValue);
                    if (!result.IsValid)
                    {
                        errors.AddRange(result.Errors.Select(e => $"{propertyName}: {e}"));
                    }
                }
                else
                {
                    errors.Add($"Property '{propertyName}' not found");
                }
                continue;
            }

            var propertyValue = property.GetValue(value);
            var validationResult = validator.Validate(propertyValue);
            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors.Select(e => $"{propertyName}: {e}"));
            }
        }

        if (errors.Count > 0)
        {
            return ValidationResult.Failure(errors);
        }

        // Apply object-level validations
        if (value is T typedValue)
        {
            var objectResult = ValidateInternal(typedValue);
            return objectResult.IsValid 
                ? ValidationResult.Success(typedValue) 
                : objectResult;
        }

        return ValidationResult.Failure(GetErrorMessage($"Value must be of type {typeof(T).Name}"));
    }

    public ObjectValidator<T> RequireProperty(string propertyName)
    {
        _validations.Add(value =>
        {
            var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return ValidationResult.Failure(GetErrorMessage($"Property '{propertyName}' is required"));
            }

            var propertyValue = property.GetValue(value);
            return propertyValue != null 
                ? ValidationResult.Success() 
                : ValidationResult.Failure(GetErrorMessage($"Property '{propertyName}' is required"));
        });
        return this;
    }
}

/// <summary>
/// Generic object validator that works with dictionaries and anonymous objects
/// </summary>
public class DynamicObjectValidator : BaseValidator<object>
{
    private readonly Dictionary<string, IValidator<object>> _schema = new();

    public DynamicObjectValidator(Dictionary<string, IValidator<object>> schema)
    {
        _schema = schema;
    }

    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Failure(GetErrorMessage("Object cannot be null"));
        }

        var errors = new List<string>();

        // Handle different object types
        Dictionary<string, object?> properties;
        
        if (value is IDictionary<string, object> dict)
        {
            properties = dict.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
        }
        else
        {
            // Use reflection for regular objects
            properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p.GetValue(value));
        }

        // Validate each property in the schema
        foreach (var (propertyName, validator) in _schema)
        {
            var propertyValue = properties.TryGetValue(propertyName, out var val) ? val : null;
            var result = validator.Validate(propertyValue);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors.Select(e => $"{propertyName}: {e}"));
            }
        }

        if (errors.Count > 0)
        {
            return ValidationResult.Failure(errors);
        }

        return ValidateInternal(value);
    }
} 