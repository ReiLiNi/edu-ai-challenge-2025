using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ValidationFramework;
using ValidationFramework.Core;
using ValidationFramework.Coverage;
using ValidationFramework.DependencyInjection;
using ValidationFramework.Examples;

// Create a host builder to demonstrate dependency injection
var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register the validation framework
        services.AddValidationFramework(options =>
        {
            options.StopOnFirstError = false;
            options.IncludePropertyPaths = true;
        });
        
        // Register specific validators
        services.AddValidator<User>(ExampleValidators.CreateUserValidator());
        services.AddValidator<Address>(ExampleValidators.CreateAddressValidator());
        
        // Register custom services
        services.AddTransient<IUserService, UserService>();
    });

var host = hostBuilder.Build();

// Run examples without dependency injection
Console.WriteLine("=== Standalone Validation Framework Demo ===");
UsageExamples.RunExamples();

// Run examples with dependency injection
Console.WriteLine("\n=== Dependency Injection Demo ===");
var userService = host.Services.GetRequiredService<IUserService>();
await userService.ProcessUserAsync();

// Run advanced examples as well
AdvancedExamples.RunAdvancedExamples();

// Run tests to demonstrate testability
Console.WriteLine("\n=== Running Tests ===");
ValidationFramework.Tests.ValidationTests.RunAllTests();
ValidationFramework.Tests.PerformanceTests.RunPerformanceTests();
ValidationFramework.Tests.IntegrationTests.RunIntegrationTests();

// Generate test coverage report
Console.WriteLine("\n=== Test Coverage Report ===");
ValidationFramework.Coverage.TestCoverageReport.GenerateReport();

Console.WriteLine("\n=== Validation Framework Demo Complete ===");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

// All class and interface declarations must come after top-level statements

/// <summary>
/// Example service that uses validation through dependency injection
/// </summary>
public interface IUserService
{
    Task ProcessUserAsync();
}

public class UserService : IUserService
{
    private readonly IValidationService _validationService;
    private readonly IValidator<User> _userValidator;
    
    public UserService(IValidationService validationService, IValidator<User> userValidator)
    {
        _validationService = validationService;
        _userValidator = userValidator;
    }
    
    public async Task ProcessUserAsync()
    {
        Console.WriteLine("Processing user with dependency injection...");
        
        // Simulate receiving user data from API
        var userData = new User(
            Id: "api-user-123",
            Name: "Jane Smith",
            Email: "jane.smith@company.com",
            Age: 28,
            IsActive: true,
            Tags: new List<string> { "admin", "power-user" },
            Address: new Address(
                Street: "456 Oak Avenue",
                City: "Somewhere",
                PostalCode: "67890",
                Country: "Canada"
            ),
            Metadata: new Dictionary<string, object>
            {
                { "department", "Engineering" },
                { "level", "Senior" }
            }
        );
        
        // Validate using the injected validator
        var result = _userValidator.Validate(userData);
        
        if (result.IsValid)
        {
            Console.WriteLine("✓ User validation passed!");
            Console.WriteLine($"Processing user: {userData.Name} ({userData.Email})");
            
            // Simulate processing...
            await Task.Delay(100);
            Console.WriteLine("User processing completed successfully.");
        }
        else
        {
            Console.WriteLine("✗ User validation failed:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Path}: {error.Message}");
            }
        }
        
        // Example of using validation service directly
        Console.WriteLine("\nUsing validation service directly:");
        var directResult = _validationService.Validate(userData);
        Console.WriteLine($"Direct validation result: {directResult.IsValid}");
        
        // Example of TryValidate pattern
        if (_validationService.TryValidate(userData, out var validatedUser))
        {
            Console.WriteLine($"Validated user: {validatedUser?.Name}");
        }
    }
}

/// <summary>
/// Example of advanced validation scenarios
/// </summary>
public static class AdvancedExamples
{
    public static void RunAdvancedExamples()
    {
        Console.WriteLine("\n=== Advanced Validation Scenarios ===");
        
        // Conditional validation
        ConditionalValidationExample();
        
        // Cross-field validation
        CrossFieldValidationExample();
        
        // Collection validation with custom rules
        CollectionValidationExample();
    }
    
    private static void ConditionalValidationExample()
    {
        Console.WriteLine("1. Conditional Validation:");
        
        // Validator that requires address only if user is active
        var conditionalValidator = Schema.Object<User>()
            .Property(u => u.Id, Schema.String().NotEmpty())
            .Property(u => u.Name, Schema.String().MinLength(2))
            .Property(u => u.Email, Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            .Property(u => u.IsActive, Schema.Boolean())
            .Property(u => u.Address, Schema.Custom<Address?>((address, path) =>
            {
                // This would normally be done with access to the full object
                // For demonstration, we'll show the concept
                return address is not null 
                    ? ExampleValidators.CreateAddressValidator().Validate(address, path)
                    : ValidationResult.Success();
            }));
        
        var userWithoutAddress = new User("1", "Test User", "test@example.com", null, false, 
            new List<string>(), null, null);
        
        var result = conditionalValidator.Validate(userWithoutAddress);
        Console.WriteLine($"User without address (inactive): {result.IsValid}");
        
        Console.WriteLine();
    }
    
    private static void CrossFieldValidationExample()
    {
        Console.WriteLine("2. Cross-Field Validation:");
        
        // Password confirmation example
        var passwordValidator = Schema.Custom<(string Password, string PasswordConfirmation)>(
            (passwords, path) =>
            {
                if (passwords.Password != passwords.PasswordConfirmation)
                    return ValidationResult.Failure(path, "Passwords do not match");
                
                if (passwords.Password.Length < 8)
                    return ValidationResult.Failure(path, "Password must be at least 8 characters");
                
                return ValidationResult.Success();
            });
        
        var validPasswords = ("SecurePass123", "SecurePass123");
        var invalidPasswords = ("SecurePass123", "DifferentPass");
        
        Console.WriteLine($"Matching passwords: {passwordValidator.Validate(validPasswords).IsValid}");
        Console.WriteLine($"Non-matching passwords: {passwordValidator.Validate(invalidPasswords).IsValid}");
        
        Console.WriteLine();
    }
    
    private static void CollectionValidationExample()
    {
        Console.WriteLine("3. Collection Validation:");
        
        // Validate that a collection contains unique items
        var uniqueStringValidator = Schema.Custom<List<string>>((items, path) =>
        {
            if (items is null) return ValidationResult.Success();
            
            var duplicates = items.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            
            return duplicates.Any() 
                ? ValidationResult.Failure(path, $"Duplicate items found: {string.Join(", ", duplicates)}")
                : ValidationResult.Success();
        });
        
        var uniqueList = new List<string> { "a", "b", "c" };
        var duplicateList = new List<string> { "a", "b", "a" };
        
        Console.WriteLine($"Unique list: {uniqueStringValidator.Validate(uniqueList).IsValid}");
        var duplicateResult = uniqueStringValidator.Validate(duplicateList);
        Console.WriteLine($"Duplicate list: {duplicateResult.IsValid}");
        if (!duplicateResult.IsValid)
        {
            Console.WriteLine($"Error: {duplicateResult.Errors.First().Message}");
        }
        
        Console.WriteLine();
    }
} 