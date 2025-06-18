using System.Text.RegularExpressions;
using ValidationFramework.Core;

namespace ValidationFramework.Validators;

/// <summary>
/// Validator for string values with fluent configuration
/// </summary>
public class StringValidator : BaseValidator<string>, IFluentValidator<StringValidator, string>
{
    private int? _minLength;
    private int? _maxLength;
    private Regex? _pattern;
    private bool _allowEmpty = true;
    
    public StringValidator MinLength(int minLength)
    {
        _minLength = minLength;
        return this;
    }
    
    public StringValidator MaxLength(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }
    
    public StringValidator Pattern(string pattern) => Pattern(new Regex(pattern));
    
    public StringValidator Pattern(Regex pattern)
    {
        _pattern = pattern;
        return this;
    }
    
    public StringValidator NotEmpty()
    {
        _allowEmpty = false;
        return this;
    }
    
    public StringValidator WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<string> Optional() => new(this);
    
    public override ValidationResult Validate(string? value, string path = "")
    {
        if (value is null)
            return Failure(path, GetErrorMessage("Value cannot be null"));
        
        if (!_allowEmpty && string.IsNullOrWhiteSpace(value))
            return Failure(path, GetErrorMessage("Value cannot be empty"));
        
        if (_minLength.HasValue && value.Length < _minLength.Value)
            return Failure(path, GetErrorMessage($"Value must be at least {_minLength.Value} characters long"));
        
        if (_maxLength.HasValue && value.Length > _maxLength.Value)
            return Failure(path, GetErrorMessage($"Value must be at most {_maxLength.Value} characters long"));
        
        if (_pattern is not null && !_pattern.IsMatch(value))
            return Failure(path, GetErrorMessage($"Value does not match the required pattern"));
        
        return Success();
    }
}

/// <summary>
/// Validator for numeric values with fluent configuration
/// </summary>
public class NumberValidator : BaseValidator<double?>, IFluentValidator<NumberValidator, double?>
{
    private double? _min;
    private double? _max;
    private bool _isInteger;
    
    public NumberValidator Min(double min)
    {
        _min = min;
        return this;
    }
    
    public NumberValidator Max(double max)
    {
        _max = max;
        return this;
    }
    
    public NumberValidator Integer()
    {
        _isInteger = true;
        return this;
    }
    
    public NumberValidator WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<double?> Optional() => new(this);
    
    public override ValidationResult Validate(double? value, string path = "")
    {
        if (!value.HasValue)
            return Failure(path, GetErrorMessage("Value cannot be null"));
        
        var numValue = value.Value;
        
        if (_isInteger && numValue != Math.Floor(numValue))
            return Failure(path, GetErrorMessage("Value must be an integer"));
        
        if (_min.HasValue && numValue < _min.Value)
            return Failure(path, GetErrorMessage($"Value must be at least {_min.Value}"));
        
        if (_max.HasValue && numValue > _max.Value)
            return Failure(path, GetErrorMessage($"Value must be at most {_max.Value}"));
        
        return Success();
    }
}

/// <summary>
/// Validator for integer values with fluent configuration
/// </summary>
public class IntegerValidator : BaseValidator<int?>, IFluentValidator<IntegerValidator, int?>
{
    private int? _min;
    private int? _max;
    
    public IntegerValidator Min(int min)
    {
        _min = min;
        return this;
    }
    
    public IntegerValidator Max(int max)
    {
        _max = max;
        return this;
    }
    
    public IntegerValidator WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<int?> Optional() => new(this);
    
    public override ValidationResult Validate(int? value, string path = "")
    {
        if (!value.HasValue)
            return Failure(path, GetErrorMessage("Value cannot be null"));
        
        var intValue = value.Value;
        
        if (_min.HasValue && intValue < _min.Value)
            return Failure(path, GetErrorMessage($"Value must be at least {_min.Value}"));
        
        if (_max.HasValue && intValue > _max.Value)
            return Failure(path, GetErrorMessage($"Value must be at most {_max.Value}"));
        
        return Success();
    }
}

/// <summary>
/// Validator for boolean values
/// </summary>
public class BooleanValidator : BaseValidator<bool?>, IFluentValidator<BooleanValidator, bool?>
{
    public BooleanValidator WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<bool?> Optional() => new(this);
    
    public override ValidationResult Validate(bool? value, string path = "")
    {
        if (!value.HasValue)
            return Failure(path, GetErrorMessage("Value cannot be null"));
        
        return Success();
    }
}

/// <summary>
/// Validator for DateTime values with fluent configuration
/// </summary>
public class DateValidator : BaseValidator<DateTime?>, IFluentValidator<DateValidator, DateTime?>
{
    private DateTime? _minDate;
    private DateTime? _maxDate;
    
    public DateValidator MinDate(DateTime minDate)
    {
        _minDate = minDate;
        return this;
    }
    
    public DateValidator MaxDate(DateTime maxDate)
    {
        _maxDate = maxDate;
        return this;
    }
    
    public DateValidator WithMessage(string message)
    {
        SetCustomMessage(message);
        return this;
    }
    
    public OptionalValidator<DateTime?> Optional() => new(this);
    
    public override ValidationResult Validate(DateTime? value, string path = "")
    {
        if (!value.HasValue)
            return Failure(path, GetErrorMessage("Value cannot be null"));
        
        var dateValue = value.Value;
        
        if (_minDate.HasValue && dateValue < _minDate.Value)
            return Failure(path, GetErrorMessage($"Date must be on or after {_minDate.Value:yyyy-MM-dd}"));
        
        if (_maxDate.HasValue && dateValue > _maxDate.Value)
            return Failure(path, GetErrorMessage($"Date must be on or before {_maxDate.Value:yyyy-MM-dd}"));
        
        return Success();
    }
} 