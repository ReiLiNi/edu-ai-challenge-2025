using System.Text.RegularExpressions;
using ValidationFramework;
using ValidationFramework.Core;
using ValidationFramework.Validators;

namespace ValidationFramework.Examples;

/// <summary>
/// Example model classes demonstrating validation usage
/// </summary>

/// <summary>
/// Represents a physical address
/// </summary>
/// <param name="Street">The street name and number</param>
/// <param name="City">The city</param>
/// <param name="PostalCode">The postal code</param>
/// <param name="Country">The country</param>
public record Address(
    string Street,
    string City,
    string PostalCode,
    string Country
);

/// <summary>
/// Represents a user of the system
/// </summary>
/// <param name="Id">The unique identifier for the user</param>
/// <param name="Name">The user's full name</param>
/// <param name="Email">The user's email address</param>
/// <param name="Age">The user's age (optional)</param>
/// <param name="IsActive">Indicates if the user's account is active</param>
/// <param name="Tags">A list of tags associated with the user</param>
/// <param name="Address">The user's address (optional)</param>
/// <param name="Metadata">A dictionary for additional user data</param>
public record User(
    string Id,
    string Name,
    string Email,
    int? Age,
    bool IsActive,
    List<string> Tags,
    Address? Address,
    Dictionary<string, object>? Metadata
);

/// <summary>
/// Example validators demonstrating framework capabilities
/// </summary>
public static class ExampleValidators
{
    /// <summary>
    /// Creates an address validator matching the JavaScript schema
    /// </summary>
    public static ObjectValidator<Address> CreateAddressValidator()
    {
        return Schema.Object<Address>()
            .Property(nameof(Address.Street), Schema.String().NotEmpty())
            .Property(nameof(Address.City), Schema.String().NotEmpty())
            .Property(nameof(Address.PostalCode), 
                Schema.String()
                    .Pattern(@"^\d{5}$")
                    .WithMessage("Postal code must be 5 digits"))
            .Property(nameof(Address.Country), Schema.String().NotEmpty());
    }
    
    /// <summary>
    /// Creates a user validator matching the JavaScript schema
    /// </summary>
    public static ObjectValidator<User> CreateUserValidator()
    {
        var addressValidator = CreateAddressValidator();
        
        return Schema.Object<User>()
            .Property(nameof(User.Id), 
                Schema.String()
                    .NotEmpty()
                    .WithMessage("ID must be a string"))
            .Property(nameof(User.Name), 
                Schema.String()
                    .MinLength(2)
                    .MaxLength(50))
            .Property(nameof(User.Email), 
                Schema.String()
                    .Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
                    .WithMessage("Invalid email format"))
            .Property(nameof(User.Age), 
                Schema.Integer()
                    .Min(0)
                    .Max(150)
                    .Optional())
            .Property(nameof(User.IsActive), Schema.Boolean())
            .Property(nameof(User.Tags), 
                Schema.Array(Schema.String().NotEmpty()))
            .Property(nameof(User.Address), addressValidator.Optional())
            .Property(nameof(User.Metadata), 
                Schema.Custom<Dictionary<string, object>?>(
                    value => value is null or { Count: >= 0 },
                    "Metadata must be a valid dictionary")
                    .Optional());
    }
    
    /// <summary>
    /// Alternative approach using expression syntax for type safety
    /// </summary>
    public static ObjectValidator<User> CreateUserValidatorWithExpressions()
    {
        var addressValidator = CreateAddressValidator();
        
        return Schema.Object<User>()
            .Property(u => u.Id, Schema.String().NotEmpty().WithMessage("ID must be a string"))
            .Property(u => u.Name, Schema.String().MinLength(2).MaxLength(50))
            .Property(u => u.Email, Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            .Property(u => u.Age, Schema.Integer().Min(0).Max(150).Optional())
            .Property(u => u.IsActive, Schema.Boolean())
            .Property(u => u.Tags, Schema.Array(Schema.String().NotEmpty()))
            .Property(u => u.Address, addressValidator.Optional());
    }
}

/// <summary>
/// Complete usage examples
/// </summary>
public static class UsageExamples
{
    public static void RunExamples()
    {
        Console.WriteLine("=== Validation Framework Examples ===\n");
        
        // Example 1: Basic validation
        BasicValidationExample();
        
        // Example 2: Complex object validation
        ComplexObjectValidationExample();
        
        // Example 3: Custom validators
        CustomValidatorExample();
        
        // Example 4: Fluent validation patterns
        FluentValidationPatternsExample();
        
        // Example 5: Error handling and reporting
        ErrorHandlingExample();
    }
    
    /// <summary>
    /// Demonstrates basic primitive validation
    /// </summary>
    public static void BasicValidationExample()
    {
        Console.WriteLine("1. Basic Validation Examples:");
        
        // String validation
        var stringValidator = Schema.String().MinLength(3).MaxLength(10);
        
        var validString = stringValidator.Validate("Hello");
        Console.WriteLine($"Valid string: {validString.IsValid}");
        
        var invalidString = stringValidator.Validate("Hi");
        Console.WriteLine($"Invalid string: {invalidString.IsValid}");
        Console.WriteLine($"Error: {invalidString.Errors.First().Message}");
        
        // Number validation
        var numberValidator = Schema.Integer().Min(0).Max(100);
        
        var validNumber = numberValidator.Validate(42);
        Console.WriteLine($"Valid number: {validNumber.IsValid}");
        
        var invalidNumber = numberValidator.Validate(-5);
        Console.WriteLine($"Invalid number: {invalidNumber.IsValid}");
        Console.WriteLine($"Error: {invalidNumber.Errors.First().Message}");
        
        Console.WriteLine();
    }
    
    /// <summary>
    /// Demonstrates complex object validation matching the JavaScript example
    /// </summary>
    public static void ComplexObjectValidationExample()
    {
        Console.WriteLine("2. Complex Object Validation Example:");
        
        var userValidator = ExampleValidators.CreateUserValidator();
        
        // Valid user data (matching the JavaScript example)
        var validUser = new User(
            Id: "12345",
            Name: "John Doe",
            Email: "john@example.com",
            Age: null, // Optional field
            IsActive: true,
            Tags: new List<string> { "developer", "designer" },
            Address: new Address(
                Street: "123 Main St",
                City: "Anytown",
                PostalCode: "12345",
                Country: "USA"
            ),
            Metadata: null // Optional field
        );
        
        var validResult = userValidator.Validate(validUser);
        Console.WriteLine($"Valid user: {validResult.IsValid}");
        
        // Invalid user data
        var invalidUser = new User(
            Id: "", // Invalid: empty
            Name: "J", // Invalid: too short
            Email: "invalid-email", // Invalid: bad format
            Age: -5, // Invalid: negative
            IsActive: true,
            Tags: new List<string> { "" }, // Invalid: empty tag
            Address: new Address(
                Street: "123 Main St",
                City: "Anytown",
                PostalCode: "1234", // Invalid: wrong format
                Country: "USA"
            ),
            Metadata: null
        );
        
        var invalidResult = userValidator.Validate(invalidUser);
        Console.WriteLine($"Invalid user: {invalidResult.IsValid}");
        Console.WriteLine("Errors:");
        foreach (var error in invalidResult.Errors)
        {
            Console.WriteLine($"  {error.Path}: {error.Message}");
        }
        
        Console.WriteLine();
    }
    
    /// <summary>
    /// Demonstrates custom validator creation
    /// </summary>
    public static void CustomValidatorExample()
    {
        Console.WriteLine("3. Custom Validator Examples:");
        
        // Custom validator with predicate
        var evenNumberValidator = Schema.Custom<int>(
            value => value % 2 == 0,
            "Number must be even"
        );
        
        var evenResult = evenNumberValidator.Validate(4);
        Console.WriteLine($"Even number (4): {evenResult.IsValid}");
        
        var oddResult = evenNumberValidator.Validate(3);
        Console.WriteLine($"Odd number (3): {oddResult.IsValid}");
        Console.WriteLine($"Error: {oddResult.Errors.First().Message}");
        
        // Custom validator with complex logic
        var passwordValidator = Schema.Custom<string>((password, path) =>
        {
            if (password is null)
                return ValidationResult.Failure(path, "Password cannot be null");
            
            var errors = new List<ValidationError>();
            
            if (password.Length < 8)
                errors.Add(new ValidationError(path, "Password must be at least 8 characters"));
            
            if (!password.Any(char.IsUpper))
                errors.Add(new ValidationError(path, "Password must contain an uppercase letter"));
            
            if (!password.Any(char.IsDigit))
                errors.Add(new ValidationError(path, "Password must contain a digit"));
            
            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        });
        
        var weakPassword = passwordValidator.Validate("weak");
        Console.WriteLine($"Weak password: {weakPassword.IsValid}");
        foreach (var error in weakPassword.Errors)
        {
            Console.WriteLine($"  {error.Message}");
        }
        
        var strongPassword = passwordValidator.Validate("StrongPass123");
        Console.WriteLine($"Strong password: {strongPassword.IsValid}");
        
        Console.WriteLine();
    }
    
    /// <summary>
    /// Demonstrates fluent validation patterns
    /// </summary>
    public static void FluentValidationPatternsExample()
    {
        Console.WriteLine("4. Fluent Validation Patterns:");
        
        // Chain multiple validations
        var emailValidator = Schema.String()
            .NotEmpty()
            .MinLength(5)
            .MaxLength(100)
            .Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
            .WithMessage("Please provide a valid email address");
        
        var validEmail = emailValidator.Validate("user@example.com");
        Console.WriteLine($"Valid email: {validEmail.IsValid}");
        
        var invalidEmail = emailValidator.Validate("invalid");
        Console.WriteLine($"Invalid email: {invalidEmail.IsValid}");
        Console.WriteLine($"Error: {invalidEmail.Errors.First().Message}");
        
        // Optional fields
        var optionalAgeValidator = Schema.Integer().Min(0).Max(150).Optional();
        
        var nullAge = optionalAgeValidator.Validate(null);
        Console.WriteLine($"Null age (optional): {nullAge.IsValid}");
        
        var validAge = optionalAgeValidator.Validate(25);
        Console.WriteLine($"Valid age: {validAge.IsValid}");
        
        Console.WriteLine();
    }
    
    /// <summary>
    /// Demonstrates error handling and structured results
    /// </summary>
    public static void ErrorHandlingExample()
    {
        Console.WriteLine("5. Error Handling and Reporting:");
        
        var userValidator = ExampleValidators.CreateUserValidator();
        
        var invalidUser = new User(
            Id: "",
            Name: "",
            Email: "bad-email",
            Age: null,
            IsActive: true,
            Tags: new List<string>(),
            Address: null,
            Metadata: null
        );
        
        var result = userValidator.Validate(invalidUser);
        
        Console.WriteLine($"Validation result: {result.IsValid}");
        Console.WriteLine($"Number of errors: {result.Errors.Count}");
        Console.WriteLine("\nDetailed error report:");
        Console.WriteLine(result.ToString());
        
        // TryValidate pattern
        if (userValidator.TryValidate(invalidUser, out var validatedUser))
        {
            Console.WriteLine("User is valid");
        }
        else
        {
            Console.WriteLine("User validation failed - cannot proceed");
        }
        
        Console.WriteLine();
    }
} 