using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ValidationFramework.Core;
using ValidationFramework.Examples;
using ValidationFramework.Validators;

namespace ValidationFramework.Tests;

[MemoryDiagnoser]
[SimpleJob]
public class ValidationBenchmarks
{
    private readonly StringValidator _stringValidator;
    private readonly IntegerValidator _integerValidator;
    private readonly IValidator<User> _userValidator;
    private readonly User _validUser;
    private readonly User _invalidUser;
    private readonly string _validString;
    private readonly string _invalidString;

    public ValidationBenchmarks()
    {
        _stringValidator = new StringValidator()
            .NotEmpty()
            .MinLength(3)
            .MaxLength(50)
            .Pattern(@"^[A-Za-z0-9@._-]+$");

        _integerValidator = new IntegerValidator()
            .Min(0)
            .Max(100);

        _userValidator = ExampleValidators.CreateUserValidator();

        _validUser = new User(
            "123",
            "John Doe",
            "john@example.com",
            25,
            true,
            new List<string> { "admin", "user" },
            new Address("123 Main St", "City", "12345", "USA"),
            new Dictionary<string, object> { { "role", "admin" } }
        );

        _invalidUser = new User(
            "",
            "J",
            "invalid-email",
            -1,
            false,
            new List<string>(),
            null,
            null
        );

        _validString = "valid.string@test.com";
        _invalidString = "";
    }

    [Benchmark]
    public ValidationResult ValidateValidString()
    {
        return _stringValidator.Validate(_validString);
    }

    [Benchmark]
    public ValidationResult ValidateInvalidString()
    {
        return _stringValidator.Validate(_invalidString);
    }

    [Benchmark]
    public ValidationResult ValidateValidInteger()
    {
        return _integerValidator.Validate(50);
    }

    [Benchmark]
    public ValidationResult ValidateInvalidInteger()
    {
        return _integerValidator.Validate(-10);
    }

    [Benchmark]
    public ValidationResult ValidateValidUser()
    {
        return _userValidator.Validate(_validUser);
    }

    [Benchmark]
    public ValidationResult ValidateInvalidUser()
    {
        return _userValidator.Validate(_invalidUser);
    }

    [Benchmark]
    public bool TryValidateString()
    {
        return _stringValidator.TryValidate(_validString, out _);
    }

    [Benchmark]
    public ValidationResult CreateStringValidator()
    {
        var validator = new StringValidator()
            .NotEmpty()
            .MinLength(5)
            .MaxLength(100);
        
        return validator.Validate("test string");
    }

    [Benchmark]
    public ValidationResult ChainedValidation()
    {
        var validator = new StringValidator()
            .NotEmpty()
            .MinLength(3)
            .MaxLength(50)
            .Pattern(@"^[A-Za-z0-9@._-]+$")
            .WithMessage("Invalid format");
        
        return validator.Validate("user123@test.com");
    }

    [Benchmark]
    public ValidationResult ComplexObjectValidation()
    {
        var addressValidator = new ObjectValidator<Address>()
            .Property(a => a.Street, new StringValidator().NotEmpty())
            .Property(a => a.City, new StringValidator().NotEmpty())
            .Property(a => a.PostalCode, new StringValidator().Pattern(@"^\d{5}$"))
            .Property(a => a.Country, new StringValidator().NotEmpty());

        var address = new Address("123 Main St", "Anytown", "12345", "USA");
        return addressValidator.Validate(address);
    }

    [Benchmark]
    public ValidationResult ArrayValidation()
    {
        var arrayValidator = new ArrayValidator<string>(new StringValidator().MinLength(3))
            .MinLength(1)
            .MaxLength(10);

        var array = new[] { "item1", "item2", "item3" };
        return arrayValidator.Validate(array);
    }

    [Benchmark]
    public ValidationResult OptionalValidation()
    {
        var optionalValidator = new StringValidator()
            .MinLength(5)
            .Optional();

        return optionalValidator.Validate(null);
    }

    // Comparison benchmarks with different approaches
    [Benchmark]
    public bool SimpleStringValidation()
    {
        var input = "test@example.com";
        return !string.IsNullOrEmpty(input) && 
               input.Length >= 3 && 
               input.Length <= 50 && 
               input.Contains('@');
    }

    [Benchmark]
    public ValidationResult MultipleValidationCalls()
    {
        var results = new[]
        {
            _stringValidator.Validate("test1"),
            _stringValidator.Validate("test2"),
            _stringValidator.Validate("test3"),
            _stringValidator.Validate("test4"),
            _stringValidator.Validate("test5")
        };

        return results.FirstOrDefault(r => !r.IsValid) ?? ValidationResult.Success();
    }
}

// Helper class to run benchmarks
public static class BenchmarkRunner
{
    public static void RunBenchmarks()
    {
        Console.WriteLine("Running validation framework benchmarks...");
        BenchmarkDotNet.Running.BenchmarkRunner.Run<ValidationBenchmarks>();
    }
} 