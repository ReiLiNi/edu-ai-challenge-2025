using System.Text.RegularExpressions;

namespace ValidationFramework;

/// <summary>
/// Base implementation for all validators
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public abstract class BaseValidator<T> : IValidator<T>
{
    protected readonly List<Func<T, ValidationResult>> _validations = new();
    protected string? _customMessage;
    protected bool _isOptional;

    public abstract ValidationResult Validate(object? value);

    public virtual IValidator<T> WithMessage(string message)
    {
        _customMessage = message;
        return this;
    }

    public virtual IValidator<T?> Optional()
    {
        return new OptionalValidator<T>(this);
    }

    public virtual IValidator<object> AsBase()
    {
        return new CastingValidator<T, object>(this);
    }

    protected ValidationResult ValidateInternal(T value)
    {
        var errors = new List<string>();

        foreach (var validation in _validations)
        {
            var result = validation(value);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Count == 0 
            ? ValidationResult.Success(value) 
            : ValidationResult.Failure(errors);
    }

    protected string GetErrorMessage(string defaultMessage)
    {
        return _customMessage ?? defaultMessage;
    }
}

/// <summary>
/// Wrapper for optional validators
/// </summary>
/// <typeparam name="T">The underlying type</typeparam>
public class OptionalValidator<T> : IValidator<T?>
{
    private readonly IValidator<T> _innerValidator;

    public OptionalValidator(IValidator<T> innerValidator)
    {
        _innerValidator = innerValidator;
    }

    public ValidationResult Validate(object? value)
    {
        if (value == null)
        {
            return ValidationResult.Success(null);
        }

        return _innerValidator.Validate(value);
    }

    public IValidator<T?> WithMessage(string message)
    {
        _innerValidator.WithMessage(message);
        return this;
    }

    public IValidator<T?> Optional()
    {
        return this; // Already optional
    }

    public IValidator<object> AsBase()
    {
        return new CastingValidator<T?, object>(this);
    }
}

internal class CastingValidator<TFrom, TTo> : IValidator<TTo>
{
    private readonly IValidator<TFrom> _inner;

    public CastingValidator(IValidator<TFrom> inner)
    {
        _inner = inner;
    }

    public ValidationResult Validate(object? value)
    {
        return _inner.Validate(value);
    }

    public IValidator<TTo> WithMessage(string message)
    {
        _inner.WithMessage(message);
        return this;
    }

    public IValidator<TTo?> Optional()
    {
        // This is a bit tricky, we need a new OptionalValidator that wraps the casting validator
        return new OptionalValidator<TTo>(this);
    }

    public IValidator<object> AsBase()
    {
        return new CastingValidator<TTo, object>(this);
    }
} 