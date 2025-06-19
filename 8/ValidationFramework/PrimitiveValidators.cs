using System.Text.RegularExpressions;

namespace ValidationFramework;

/// <summary>
/// String validator with fluent API
/// </summary>
public class StringValidator : BaseValidator<string>
{
    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Failure(GetErrorMessage("Value cannot be null"));
        }

        if (value is not string stringValue)
        {
            return ValidationResult.Failure(GetErrorMessage("Value must be a string"));
        }

        return ValidateInternal(stringValue);
    }

    public StringValidator MinLength(int minLength)
    {
        _validations.Add(value => value.Length >= minLength 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"String must be at least {minLength} characters long")));
        return this;
    }

    public StringValidator MaxLength(int maxLength)
    {
        _validations.Add(value => value.Length <= maxLength 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"String must be at most {maxLength} characters long")));
        return this;
    }

    public StringValidator Pattern(Regex regex)
    {
        _validations.Add(value => regex.IsMatch(value) 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"String does not match the required pattern")));
        return this;
    }

    public StringValidator NotEmpty()
    {
        _validations.Add(value => !string.IsNullOrEmpty(value) 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage("String cannot be empty")));
        return this;
    }

    public StringValidator Email()
    {
        var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.IgnoreCase);
        Pattern(emailRegex);
        return (StringValidator)WithMessage("Invalid email format");
    }
}

/// <summary>
/// Number validator for integers and decimals
/// </summary>
public class NumberValidator : BaseValidator<decimal>
{
    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Failure(GetErrorMessage("Value cannot be null"));
        }

        decimal numericValue;
        if (value is decimal d)
        {
            numericValue = d;
        }
        else if (value is int i)
        {
            numericValue = i;
        }
        else if (value is double dbl)
        {
            numericValue = (decimal)dbl;
        }
        else if (value is float f)
        {
            numericValue = (decimal)f;
        }
        else if (value is long l)
        {
            numericValue = l;
        }
        else if (decimal.TryParse(value.ToString(), out decimal parsed))
        {
            numericValue = parsed;
        }
        else
        {
            return ValidationResult.Failure(GetErrorMessage("Value must be a number"));
        }

        return ValidateInternal(numericValue);
    }

    public NumberValidator Min(decimal min)
    {
        _validations.Add(value => value >= min 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"Number must be at least {min}")));
        return this;
    }

    public NumberValidator Max(decimal max)
    {
        _validations.Add(value => value <= max 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"Number must be at most {max}")));
        return this;
    }

    public NumberValidator Positive()
    {
        _validations.Add(value => value > 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage("Number must be positive")));
        return this;
    }

    public NumberValidator Integer()
    {
        _validations.Add(value => value == Math.Floor(value) 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage("Number must be an integer")));
        return this;
    }
}

/// <summary>
/// Boolean validator
/// </summary>
public class BooleanValidator : BaseValidator<bool>
{
    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Failure(GetErrorMessage("Value cannot be null"));
        }

        if (value is bool boolValue)
        {
            return ValidateInternal(boolValue);
        }

        if (bool.TryParse(value.ToString(), out bool parsed))
        {
            return ValidateInternal(parsed);
        }

        return ValidationResult.Failure(GetErrorMessage("Value must be a boolean"));
    }

    public BooleanValidator IsTrue()
    {
        _validations.Add(value => value 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage("Value must be true")));
        return this;
    }

    public BooleanValidator IsFalse()
    {
        _validations.Add(value => !value 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage("Value must be false")));
        return this;
    }
}

/// <summary>
/// Date validator
/// </summary>
public class DateValidator : BaseValidator<DateTime>
{
    public override ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Failure(GetErrorMessage("Value cannot be null"));
        }

        DateTime dateValue;
        if (value is DateTime dt)
        {
            dateValue = dt;
        }
        else if (value is DateTimeOffset dto)
        {
            dateValue = dto.DateTime;
        }
        else if (DateTime.TryParse(value.ToString(), out DateTime parsed))
        {
            dateValue = parsed;
        }
        else
        {
            return ValidationResult.Failure(GetErrorMessage("Value must be a valid date"));
        }

        return ValidateInternal(dateValue);
    }

    public DateValidator After(DateTime date)
    {
        _validations.Add(value => value > date 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"Date must be after {date:yyyy-MM-dd}")));
        return this;
    }

    public DateValidator Before(DateTime date)
    {
        _validations.Add(value => value < date 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"Date must be before {date:yyyy-MM-dd}")));
        return this;
    }

    public DateValidator InRange(DateTime start, DateTime end)
    {
        _validations.Add(value => value >= start && value <= end 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(GetErrorMessage($"Date must be between {start:yyyy-MM-dd} and {end:yyyy-MM-dd}")));
        return this;
    }
} 