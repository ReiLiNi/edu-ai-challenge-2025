using System.Collections;
using System.Reflection;
using ValidationFramework.Core;

namespace ValidationFramework.Validators;

/// <summary>
/// Validator for array/collection types with item validation
/// </summary>
/// <typeparam name="T">The type of items in the array</typeparam>
public class ArrayValidator<T> : BaseValidator<IEnumerable<T>>, IFluentValidator<ArrayValidator<T>, IEnumerable<T>>
{
    private readonly IValidator<T> _itemValidator;
    private int? _minLength;
    private int? _maxLength;
    
    public ArrayValidator(IValidator<T> itemValidator)
    {
        _itemValidator = itemValidator ?? throw new ArgumentNullException(nameof(itemValidator));
    }
    
    public ArrayValidator<T> MinLength(int minLength)
    {
        _minLength = minLength;
        return this;
    }
    
    public ArrayValidator<T> MaxLength(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }
    
    public ArrayValidator<T> WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<IEnumerable<T>> Optional() => new(this);
    
    public override ValidationResult Validate(IEnumerable<T>? value, string path = "")
    {
        if (value is null)
            return Failure(path, GetErrorMessage("Array cannot be null"));
        
        var items = value.ToList();
        
        if (_minLength.HasValue && items.Count < _minLength.Value)
            return Failure(path, GetErrorMessage($"Array must contain at least {_minLength.Value} items"));
        
        if (_maxLength.HasValue && items.Count > _maxLength.Value)
            return Failure(path, GetErrorMessage($"Array must contain at most {_maxLength.Value} items"));
        
        var results = new List<ValidationResult>();
        for (int i = 0; i < items.Count; i++)
        {
            var itemPath = string.IsNullOrEmpty(path) ? $"[{i}]" : $"{path}[{i}]";
            results.Add(_itemValidator.Validate(items[i], itemPath));
        }
        
        return ValidationResult.Combine(results);
    }
}

/// <summary>
/// Validator for object types with property-level validation
/// </summary>
/// <typeparam name="T">The type of object being validated</typeparam>
public class ObjectValidator<T> : BaseValidator<T>, IFluentValidator<ObjectValidator<T>, T> where T : class
{
    private readonly Dictionary<string, IValidator<object?>> _propertyValidators = new();
    
    public ObjectValidator() { }
    
    public ObjectValidator(Dictionary<string, IValidator<object?>> propertyValidators)
    {
        _propertyValidators = propertyValidators ?? throw new ArgumentNullException(nameof(propertyValidators));
    }
    
    /// <summary>
    /// Adds a validator for a specific property
    /// </summary>
    /// <typeparam name="TProperty">The type of the property</typeparam>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="validator">The validator for the property</param>
    /// <returns>This validator for fluent chaining</returns>
    public ObjectValidator<T> Property<TProperty>(string propertyName, IValidator<TProperty> validator)
    {
        _propertyValidators[propertyName] = new PropertyValidatorWrapper<TProperty>(validator);
        return this;
    }
    
    /// <summary>
    /// Adds a validator for a specific property using expression syntax
    /// </summary>
    /// <typeparam name="TProperty">The type of the property</typeparam>
    /// <param name="propertyExpression">Expression pointing to the property</param>
    /// <param name="validator">The validator for the property</param>
    /// <returns>This validator for fluent chaining</returns>
    public ObjectValidator<T> Property<TProperty>(
        System.Linq.Expressions.Expression<Func<T, TProperty>> propertyExpression, 
        IValidator<TProperty> validator)
    {
        var propertyName = GetPropertyName(propertyExpression);
        return Property(propertyName, validator);
    }
    
    public ObjectValidator<T> WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<T> Optional() => new(this);
    
    public override ValidationResult Validate(T? value, string path = "")
    {
        if (value is null)
            return Failure(path, GetErrorMessage("Object cannot be null"));
        
        var results = new List<ValidationResult>();
        var type = typeof(T);
        
        foreach (var (propertyName, validator) in _propertyValidators)
        {
            var property = type.GetProperty(propertyName);
            if (property is null)
            {
                results.Add(Failure(path, $"Property '{propertyName}' not found on type {type.Name}"));
                continue;
            }
            
            var propertyValue = property.GetValue(value);
            var propertyPath = string.IsNullOrEmpty(path) ? propertyName : $"{path}.{propertyName}";
            
            results.Add(validator.Validate(propertyValue, propertyPath));
        }
        
        return ValidationResult.Combine(results);
    }
    
    private static string GetPropertyName<TProperty>(System.Linq.Expressions.Expression<Func<T, TProperty>> expression)
    {
        return expression.Body switch
        {
            System.Linq.Expressions.MemberExpression memberExpression => memberExpression.Member.Name,
            System.Linq.Expressions.UnaryExpression { Operand: System.Linq.Expressions.MemberExpression memberExpr } => memberExpr.Member.Name,
            _ => throw new ArgumentException("Expression must be a member expression", nameof(expression))
        };
    }
}

/// <summary>
/// Wrapper to make property validators work with object validation
/// </summary>
/// <typeparam name="T">The property type</typeparam>
internal class PropertyValidatorWrapper<T> : IValidator<object?>
{
    private readonly IValidator<T> _innerValidator;
    
    public PropertyValidatorWrapper(IValidator<T> innerValidator)
    {
        _innerValidator = innerValidator ?? throw new ArgumentNullException(nameof(innerValidator));
    }
    
    public ValidationResult Validate(object? value, string path = "")
    {
        try
        {
            var typedValue = (T?)value;
            return _innerValidator.Validate(typedValue, path);
        }
        catch (InvalidCastException)
        {
            return ValidationResult.Failure(path, $"Expected type {typeof(T).Name} but got {value?.GetType().Name ?? "null"}");
        }
    }
    
    public bool TryValidate(object? value, out object? validatedValue)
    {
        try
        {
            var typedValue = (T?)value;
            if (_innerValidator.TryValidate(typedValue, out var typedValidatedValue))
            {
                validatedValue = typedValidatedValue;
                return true;
            }
        }
        catch (InvalidCastException)
        {
            // Fall through to return false
        }
        
        validatedValue = null;
        return false;
    }
} 