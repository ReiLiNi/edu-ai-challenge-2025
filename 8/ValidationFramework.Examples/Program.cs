using ValidationFramework;
using System.Text.RegularExpressions;

namespace ValidationFramework.Examples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Validation Framework Demo ===\n");

        // Primitive type examples
        DemonstratePrimitiveValidators();
        
        // Complex type examples
        DemonstrateComplexValidators();
        
        // Real-world example (matching the JavaScript schema)
        DemonstrateRealWorldExample();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static void DemonstratePrimitiveValidators()
    {
        Console.WriteLine("=== Primitive Type Validators ===");

        // String validation
        Console.WriteLine("\n1. String Validation:");
        var stringValidator = Schema.String()
            .MinLength(2)
            .MaxLength(50)
            .WithMessage("Name must be between 2 and 50 characters");

        TestValidator(stringValidator, "John Doe", "Valid name");
        TestValidator(stringValidator, "J", "Too short name");
        TestValidator(stringValidator, new string('A', 51), "Too long name");

        // Email validation
        Console.WriteLine("\n2. Email Validation:");
        var emailValidator = Schema.String().Email();
        
        TestValidator(emailValidator, "john@example.com", "Valid email");
        TestValidator(emailValidator, "invalid-email", "Invalid email");

        // Number validation
        Console.WriteLine("\n3. Number Validation:");
        var ageValidator = Schema.Number()
            .Min(0)
            .Max(120)
            .Integer()
            .WithMessage("Age must be between 0 and 120");

        TestValidator(ageValidator, 25, "Valid age");
        TestValidator(ageValidator, -5, "Negative age");
        TestValidator(ageValidator, 25.5, "Non-integer age");

        // Boolean validation
        Console.WriteLine("\n4. Boolean Validation:");
        var activeValidator = Schema.Boolean();
        
        TestValidator(activeValidator, true, "Valid boolean");
        TestValidator(activeValidator, "not boolean", "Invalid boolean");

        // Date validation
        Console.WriteLine("\n5. Date Validation:");
        var dateValidator = Schema.Date()
            .After(new DateTime(2020, 1, 1))
            .Before(new DateTime(2030, 1, 1));

        TestValidator(dateValidator, new DateTime(2023, 6, 15), "Valid date");
        TestValidator(dateValidator, new DateTime(2019, 1, 1), "Date too old");
    }

    static void DemonstrateComplexValidators()
    {
        Console.WriteLine("\n\n=== Complex Type Validators ===");

        // Array validation
        Console.WriteLine("\n1. Array Validation:");
        var tagsValidator = Schema.Array(Schema.String().MinLength(2))
            .MinLength(1)
            .MaxLength(5)
            .WithMessage("Must have 1-5 tags, each at least 2 characters");

        TestValidator(tagsValidator, new[] { "developer", "designer" }, "Valid tags");
        TestValidator(tagsValidator, new[] { "dev", "a" }, "Tag too short");
        TestValidator(tagsValidator, new string[0], "Empty array");

        // Object validation
        Console.WriteLine("\n2. Object Validation:");
        var addressSchema = new Dictionary<string, IValidator<object>>
        {
            { "Street", Schema.String().MinLength(5).AsBase() },
            { "City", Schema.String().MinLength(2).AsBase() },
            { "PostalCode", Schema.String().Pattern(new Regex(@"^\d{5}$")).WithMessage("Postal code must be 5 digits").AsBase() },
            { "Country", Schema.String().MinLength(2).AsBase() }
        };

        var addressValidator = Schema.Object(addressSchema);
        
        var validAddress = new Dictionary<string, object>
        {
            { "Street", "123 Main St" },
            { "City", "Anytown" },
            { "PostalCode", "12345" },
            { "Country", "USA" }
        };

        var invalidAddress = new Dictionary<string, object>
        {
            { "Street", "123" },
            { "City", "A" },
            { "PostalCode", "123" },
            { "Country", "U" }
        };

        TestValidator(addressValidator, validAddress, "Valid address");
        TestValidator(addressValidator, invalidAddress, "Invalid address");
    }

    static void DemonstrateRealWorldExample()
    {
        Console.WriteLine("\n\n=== Real-World Example (User Registration) ===");

        // Define address schema
        var addressSchema = new Dictionary<string, IValidator<object>>
        {
            { "street", Schema.String().MinLength(5).AsBase() },
            { "city", Schema.String().MinLength(2).AsBase() },
            { "postalCode", Schema.String().Pattern(new Regex(@"^\d{5}$")).WithMessage("Postal code must be 5 digits").AsBase() },
            { "country", Schema.String().MinLength(2).AsBase() }
        };

        // Define user schema (matching the JavaScript example)
        var userSchema = new Dictionary<string, IValidator<object>>
        {
            { "id", Schema.String().WithMessage("ID must be a string").AsBase() },
            { "name", Schema.String().MinLength(2).MaxLength(50).AsBase() },
            { "email", Schema.String().Pattern(new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")).AsBase() },
            { "age", Schema.Number().Min(0).Optional().AsBase() },
            { "isActive", Schema.Boolean().AsBase() },
            { "tags", Schema.Array(Schema.String()).AsBase() },
            { "address", Schema.Object(addressSchema).Optional().AsBase() },
            { "metadata", Schema.Object(new Dictionary<string, IValidator<object>>()).Optional().AsBase() }
        };

        var userValidator = Schema.Object(userSchema);

        // Valid user data (matching the JavaScript example)
        var validUserData = new Dictionary<string, object>
        {
            { "id", "12345" },
            { "name", "John Doe" },
            { "email", "john@example.com" },
            { "age", 30 },
            { "isActive", true },
            { "tags", new[] { "developer", "designer" } },
            { "address", new Dictionary<string, object>
                {
                    { "street", "123 Main St" },
                    { "city", "Anytown" },
                    { "postalCode", "12345" },
                    { "country", "USA" }
                }
            }
        };

        // Invalid user data
        var invalidUserData = new Dictionary<string, object>
        {
            { "id", 123 }, // Should be string
            { "name", "J" }, // Too short
            { "email", "invalid-email" }, // Invalid format
            { "age", -5 }, // Negative age
            { "isActive", "not-boolean" }, // Invalid boolean
            { "tags", new[] { "" } }, // Empty tag
            { "address", new Dictionary<string, object>
                {
                    { "street", "123" }, // Too short
                    { "city", "A" }, // Too short
                    { "postalCode", "123" }, // Invalid format
                    { "country", "U" } // Too short
                }
            }
        };

        Console.WriteLine("\nTesting valid user data:");
        TestValidator(userValidator, validUserData, "Complete user registration");

        Console.WriteLine("\nTesting invalid user data:");
        TestValidator(userValidator, invalidUserData, "Invalid user registration");

        // Demonstrate optional fields
        var minimalUserData = new Dictionary<string, object>
        {
            { "id", "67890" },
            { "name", "Jane Smith" },
            { "email", "jane@example.com" },
            { "isActive", false },
            { "tags", new[] { "tester" } }
            // age, address, and metadata are optional
        };

        Console.WriteLine("\nTesting minimal user data (optional fields omitted):");
        TestValidator(userValidator, minimalUserData, "Minimal user registration");
    }

    static void TestValidator<T>(IValidator<T> validator, object? value, string description)
    {
        Console.WriteLine($"\nTesting: {description}");
        Console.WriteLine($"Value: {FormatValue(value)}");
        
        var result = validator.Validate(value);
        
        if (result.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ VALID");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ INVALID");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        
        Console.ResetColor();
    }

    static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            Array arr => $"[{string.Join(", ", arr.Cast<object>().Select(FormatValue))}]",
            Dictionary<string, object> dict => $"{{{string.Join(", ", dict.Take(3).Select(kvp => $"{kvp.Key}: {FormatValue(kvp.Value)}"))}}}",
            _ => value.ToString() ?? "null"
        };
    }
}

// Example classes for demonstration
public class User
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public int? Age { get; set; }
    public bool IsActive { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public Address? Address { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";
} 