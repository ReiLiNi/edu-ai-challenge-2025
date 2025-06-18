using ValidationFramework;
using ValidationFramework.Core;
using ValidationFramework.Examples;
using ValidationFramework.Validators;

namespace ValidationFramework.Tests;

/// <summary>
/// Unit tests demonstrating how to test the validation framework
/// Note: In a real project, you would use a testing framework like xUnit, NUnit, or MSTest
/// This is a simple demonstration of testing concepts
/// </summary>
public static class ValidationTests
{
    public static void RunAllTests()
    {
        Console.WriteLine("=== Running Validation Framework Tests ===\n");
        
        TestPrimitiveValidators();
        TestComplexValidators();
        TestCustomValidators();
        TestValidationResults();
        TestOptionalValidators();
        TestFluentInterface();
        TestErrorMessages();
        
        // New comprehensive test suites
        TestEdgeCases();
        TestBoundaryConditions();
        TestSpecialCharactersAndUnicode();
        TestNullAndEmptyScenarios();
        TestValidationPaths();
        TestValidatorCombinations();
        TestErrorAccumulation();
        TestThreadSafety();
        
        Console.WriteLine("=== All Tests Completed ===\n");
    }
    
    private static void TestPrimitiveValidators()
    {
        Console.WriteLine("Testing Primitive Validators:");
        
        // String validator tests
        var stringValidator = Schema.String().MinLength(3).MaxLength(10);
        
        // Act & Assert
        Assert(stringValidator.Validate("Hello").IsValid, "Valid string should pass");
        Assert(!stringValidator.Validate("Hi").IsValid, "String too short should fail");
        Assert(!stringValidator.Validate("This is too long").IsValid, "String too long should fail");
        Assert(!stringValidator.Validate(null).IsValid, "Null string should fail");
        
        // Integer validator tests
        var intValidator = Schema.Integer().Min(0).Max(100);
        
        // Act & Assert
        Assert(intValidator.Validate(50).IsValid, "Valid integer should pass");
        Assert(!intValidator.Validate(-1).IsValid, "Integer below min should fail");
        Assert(!intValidator.Validate(101).IsValid, "Integer above max should fail");
        
        // Boolean validator tests
        var boolValidator = Schema.Boolean();
        
        // Act & Assert
        Assert(boolValidator.Validate(true).IsValid, "True boolean should pass");
        Assert(boolValidator.Validate(false).IsValid, "False boolean should pass");
        Assert(!boolValidator.Validate(null).IsValid, "Null boolean should fail");
        
        // Pattern validator tests
        var emailValidator = Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        
        // Act & Assert
        Assert(emailValidator.Validate("test@example.com").IsValid, "Valid email should pass");
        Assert(!emailValidator.Validate("invalid-email").IsValid, "Invalid email should fail");
        
        Console.WriteLine("✓ Primitive validator tests passed\n");
    }
    
    private static void TestComplexValidators()
    {
        Console.WriteLine("Testing Complex Validators:");
        
        // Arrange: Array validator tests
        var stringArrayValidator = Schema.Array(Schema.String().MinLength(1));
        
        // Act & Assert
        Assert(stringArrayValidator.Validate(new[] { "a", "b", "c" }).IsValid, "Valid string array should pass");
        Assert(!stringArrayValidator.Validate(new[] { "a", "", "c" }).IsValid, "Array with empty string should fail");
        Assert(!stringArrayValidator.Validate(null).IsValid, "Null array should fail");
        
        // Arrange: Object validator tests
        var addressValidator = ExampleValidators.CreateAddressValidator();
        var validAddress = new Address("123 Main St", "City", "12345", "USA");
        var invalidAddress = new Address("", "City", "123", "USA"); // Invalid street and postal code
        
        // Act & Assert
        Assert(addressValidator.Validate(validAddress).IsValid, "Valid address should pass");
        Assert(!addressValidator.Validate(invalidAddress).IsValid, "Invalid address should fail");
        Assert(!addressValidator.Validate(null).IsValid, "Null address should fail");
        
        // Arrange: Nested object validation
        var userValidator = ExampleValidators.CreateUserValidator();
        var validUser = new User("123", "John Doe", "john@example.com", null, true,
            new List<string> { "tag1" }, validAddress, null);
        
        // Act & Assert
        Assert(userValidator.Validate(validUser).IsValid, "Valid user should pass");
        
        Console.WriteLine("✓ Complex validator tests passed\n");
    }
    
    private static void TestCustomValidators()
    {
        Console.WriteLine("Testing Custom Validators:");
        
        // Arrange: Simple predicate validator
        var evenValidator = Schema.Custom<int>(x => x % 2 == 0, "Must be even");
        
        // Act & Assert
        Assert(evenValidator.Validate(4).IsValid, "Even number should pass");
        Assert(!evenValidator.Validate(3).IsValid, "Odd number should fail");
        
        // Arrange: Complex custom validator
        var passwordValidator = Schema.Custom<string>((password, path) =>
        {
            if (password is null) return ValidationResult.Failure(path, "Password required");
            if (password.Length < 8) return ValidationResult.Failure(path, "Password too short");
            if (!password.Any(char.IsUpper)) return ValidationResult.Failure(path, "Password needs uppercase");
            return ValidationResult.Success();
        });
        
        // Act & Assert
        Assert(passwordValidator.Validate("StrongPass123").IsValid, "Strong password should pass");
        Assert(!passwordValidator.Validate("weak").IsValid, "Weak password should fail");
        
        Console.WriteLine("✓ Custom validator tests passed\n");
    }
    
    private static void TestValidationResults()
    {
        Console.WriteLine("Testing Validation Results:");
        
        // Arrange: Test successful result
        var successResult = ValidationResult.Success();
        
        // Act & Assert
        Assert(successResult.IsValid, "Success result should be valid");
        Assert(successResult.Errors.Count == 0, "Success result should have no errors");
        
        // Arrange: Test failure result
        var failureResult = ValidationResult.Failure("field", "error message");
        
        // Act & Assert
        Assert(!failureResult.IsValid, "Failure result should be invalid");
        Assert(failureResult.Errors.Count == 1, "Failure result should have one error");
        Assert(failureResult.Errors[0].Path == "field", "Error should have correct path");
        Assert(failureResult.Errors[0].Message == "error message", "Error should have correct message");
        
        // Arrange: Test combining results
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Failure("field1", "error1");
        var result3 = ValidationResult.Failure("field2", "error2");
        
        // Act
        var combinedResult = ValidationResult.Combine(result1, result2, result3);
        
        // Assert
        Assert(!combinedResult.IsValid, "Combined result with failures should be invalid");
        Assert(combinedResult.Errors.Count == 2, "Combined result should have two errors");
        
        Console.WriteLine("✓ Validation result tests passed\n");
    }
    
    private static void TestOptionalValidators()
    {
        Console.WriteLine("Testing Optional Validators:");
        
        // Arrange
        var optionalStringValidator = Schema.String().MinLength(3).Optional();
        
        // Act & Assert
        Assert(optionalStringValidator.Validate(null).IsValid, "Null should be valid for optional validator");
        Assert(optionalStringValidator.Validate("Hello").IsValid, "Valid value should pass for optional validator");
        Assert(!optionalStringValidator.Validate("Hi").IsValid, "Invalid value should fail for optional validator");
        
        // Arrange: Test TryValidate with optional
        // Act
        var tryValidateNull = optionalStringValidator.TryValidate(null, out var nullResult);
        // Assert
        Assert(tryValidateNull, "TryValidate should succeed with null");
        Assert(nullResult == null, "Validated null result should be null");
        
        // Act
        var tryValidateValid = optionalStringValidator.TryValidate("Hello", out var validResult);
        // Assert
        Assert(tryValidateValid, "TryValidate should succeed with valid value");
        Assert(validResult == "Hello", "Validated result should match input");
        
        Console.WriteLine("✓ Optional validator tests passed\n");
    }
    
    private static void TestFluentInterface()
    {
        Console.WriteLine("Testing Fluent Interface:");
        
        // Arrange: Test method chaining
        var chainedValidator = Schema.String()
            .NotEmpty()
            .MinLength(5)
            .MaxLength(20)
            .Pattern(@"^[A-Za-z]+$")
            .WithMessage("Invalid name format");
        
        // Act & Assert
        Assert(chainedValidator.Validate("ValidName").IsValid, "Valid chained validation should pass");
        Assert(!chainedValidator.Validate("123").IsValid, "Invalid chained validation should fail");
        
        // Arrange: Test that custom message is used
        // Act
        var result = chainedValidator.Validate("123");
        
        // Assert
        Assert(result.Errors.Any(e => e.Message == "Invalid name format"), "Custom message should be used");
        
        Console.WriteLine("✓ Fluent interface tests passed\n");
    }
    
    private static void TestErrorMessages()
    {
        Console.WriteLine("Testing Error Messages:");
        
        // Arrange
        var validator = Schema.String().MinLength(5).WithMessage("Custom error message");
        
        // Act
        var result = validator.Validate("Hi");
        
        // Assert
        Assert(!result.IsValid, "Validation should fail");
        Assert(result.Errors[0].Message == "Custom error message", "Custom error message should be used");
        
        // Arrange: Test path construction in nested validation
        var userValidator = Schema.Object<User>()
            .Property(nameof(User.Name), Schema.String().MinLength(2))
            .Property(nameof(User.Address), Schema.Object<Address>()
                .Property(nameof(Address.PostalCode), Schema.String().Pattern(@"^\d{5}$")));
        
        var invalidUser = new User("1", "J", "email@test.com", null, true, new List<string>(),
            new Address("Street", "City", "123", "Country"), null);
        
        // Act
        var userResult = userValidator.Validate(invalidUser);
        
        // Assert
        Assert(!userResult.IsValid, "User validation should fail");
        
        var nameError = userResult.Errors.FirstOrDefault(e => e.Path == nameof(User.Name));
        var postalError = userResult.Errors.FirstOrDefault(e => e.Path == $"{nameof(User.Address)}.{nameof(Address.PostalCode)}");
        
        Assert(nameError != null, "Should have name validation error");
        Assert(postalError != null, "Should have postal code validation error with correct path");
        
        Console.WriteLine("✓ Error message tests passed\n");
    }
    
    /// <summary>
    /// Tests edge cases that might cause unexpected behavior
    /// </summary>
    private static void TestEdgeCases()
    {
        Console.WriteLine("Testing Edge Cases:");
        
        // Arrange: String with exactly min/max length
        var stringValidator = Schema.String().MinLength(5).MaxLength(5);
        
        // Act & Assert: Boundary value testing
        Assert(stringValidator.Validate("12345").IsValid, "String with exact min/max length should pass");
        Assert(!stringValidator.Validate("1234").IsValid, "String one character short should fail");
        Assert(!stringValidator.Validate("123456").IsValid, "String one character long should fail");
        
        // Arrange: Integer boundary values
        var intValidator = Schema.Integer().Min(int.MinValue).Max(int.MaxValue);
        
        // Act & Assert
        Assert(intValidator.Validate(int.MinValue).IsValid, "Integer min value should pass");
        Assert(intValidator.Validate(int.MaxValue).IsValid, "Integer max value should pass");
        Assert(intValidator.Validate(0).IsValid, "Zero should pass");
        
        // Arrange: Number boundary values
        var numberValidator = Schema.Number().Min(double.MinValue).Max(double.MaxValue);
        
        // Act & Assert
        Assert(numberValidator.Validate(double.MinValue).IsValid, "Double min value should pass");
        Assert(numberValidator.Validate(double.MaxValue).IsValid, "Double max value should pass");
        Assert(numberValidator.Validate(0.0).IsValid, "Zero double should pass");
        Assert(numberValidator.Validate(double.Epsilon).IsValid, "Epsilon should pass");
        
        // Arrange: Date boundary values
        var dateValidator = Schema.Date().MinDate(DateTime.MinValue).MaxDate(DateTime.MaxValue);
        
        // Act & Assert
        Assert(dateValidator.Validate(DateTime.MinValue).IsValid, "DateTime min value should pass");
        Assert(dateValidator.Validate(DateTime.MaxValue).IsValid, "DateTime max value should pass");
        Assert(dateValidator.Validate(DateTime.Now).IsValid, "Current DateTime should pass");
        
        // Arrange: Empty array validation
        var arrayValidator = Schema.Array(Schema.String().NotEmpty()).MinLength(0);
        
        // Act & Assert
        Assert(arrayValidator.Validate(new string[0]).IsValid, "Empty array should pass when min length is 0");
        Assert(arrayValidator.Validate(new List<string>()).IsValid, "Empty list should pass when min length is 0");
        
        Console.WriteLine("✓ Edge case tests passed\n");
    }
    
    /// <summary>
    /// Tests boundary conditions for all validator types
    /// </summary>
    private static void TestBoundaryConditions()
    {
        Console.WriteLine("Testing Boundary Conditions:");
        
        // Arrange: Test off-by-one errors in string length
        var stringValidator = Schema.String().MinLength(3).MaxLength(10);
        
        // Act & Assert: Lower boundary
        Assert(!stringValidator.Validate("ab").IsValid, "String with 2 chars should fail (min 3)");
        Assert(stringValidator.Validate("abc").IsValid, "String with 3 chars should pass (min 3)");
        Assert(stringValidator.Validate("abcd").IsValid, "String with 4 chars should pass");
        
        // Act & Assert: Upper boundary
        Assert(stringValidator.Validate("1234567890").IsValid, "String with 10 chars should pass (max 10)");
        Assert(!stringValidator.Validate("12345678901").IsValid, "String with 11 chars should fail (max 10)");
        
        // Arrange: Test integer boundaries
        var intValidator = Schema.Integer().Min(10).Max(20);
        
        // Act & Assert
        Assert(!intValidator.Validate(9).IsValid, "9 should fail (min 10)");
        Assert(intValidator.Validate(10).IsValid, "10 should pass (min 10)");
        Assert(intValidator.Validate(20).IsValid, "20 should pass (max 20)");
        Assert(!intValidator.Validate(21).IsValid, "21 should fail (max 20)");
        
        // Arrange: Test array length boundaries
        var arrayValidator = Schema.Array(Schema.String()).MinLength(2).MaxLength(3);
        
        // Act & Assert
        Assert(!arrayValidator.Validate(new[] { "one" }).IsValid, "Array with 1 item should fail (min 2)");
        Assert(arrayValidator.Validate(new[] { "one", "two" }).IsValid, "Array with 2 items should pass (min 2)");
        Assert(arrayValidator.Validate(new[] { "one", "two", "three" }).IsValid, "Array with 3 items should pass (max 3)");
        Assert(!arrayValidator.Validate(new[] { "one", "two", "three", "four" }).IsValid, "Array with 4 items should fail (max 3)");
        
        // Arrange: Test date boundaries
        var baseDate = new DateTime(2023, 1, 1);
        var dateValidator = Schema.Date().MinDate(baseDate).MaxDate(baseDate.AddDays(30));
        
        // Act & Assert
        Assert(!dateValidator.Validate(baseDate.AddDays(-1)).IsValid, "Date before min should fail");
        Assert(dateValidator.Validate(baseDate).IsValid, "Min date should pass");
        Assert(dateValidator.Validate(baseDate.AddDays(30)).IsValid, "Max date should pass");
        Assert(!dateValidator.Validate(baseDate.AddDays(31)).IsValid, "Date after max should fail");
        
        Console.WriteLine("✓ Boundary condition tests passed\n");
    }
    
    /// <summary>
    /// Tests handling of special characters and Unicode
    /// </summary>
    private static void TestSpecialCharactersAndUnicode()
    {
        Console.WriteLine("Testing Special Characters and Unicode:");
        
        // Arrange: String with special characters
        var stringValidator = Schema.String().MinLength(1).MaxLength(50);
        
        // Act & Assert: Special characters
        Assert(stringValidator.Validate("Hello@World!").IsValid, "String with @ and ! should pass");
        Assert(stringValidator.Validate("Price: $19.99").IsValid, "String with $ and . should pass");
        Assert(stringValidator.Validate("C:\\Path\\To\\File").IsValid, "String with backslashes should pass");
        Assert(stringValidator.Validate("Line1\nLine2").IsValid, "String with newline should pass");
        Assert(stringValidator.Validate("Tab\tSeparated").IsValid, "String with tab should pass");
        
        // Act & Assert: Unicode characters
        Assert(stringValidator.Validate("Héllo Wörld").IsValid, "String with accented chars should pass");
        Assert(stringValidator.Validate("こんにちは").IsValid, "String with Japanese chars should pass");
        Assert(stringValidator.Validate("🌟⭐✨").IsValid, "String with emojis should pass");
        Assert(stringValidator.Validate("Москва").IsValid, "String with Cyrillic chars should pass");
        Assert(stringValidator.Validate("العربية").IsValid, "String with Arabic chars should pass");
        
        // Arrange: Email validator with Unicode domains
        var emailValidator = Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        
        // Act & Assert: International email addresses
        Assert(emailValidator.Validate("test@bücher.de").IsValid, "Email with Unicode domain should pass");
        Assert(emailValidator.Validate("user@xn--bcher-kva.de").IsValid, "Email with punycode domain should pass");
        
        // Arrange: Pattern validator for special cases
        var phoneValidator = Schema.String().Pattern(@"^\+?[\d\s\-\(\)]+$");
        
        // Act & Assert: Phone number patterns
        Assert(phoneValidator.Validate("+1 (555) 123-4567").IsValid, "US phone format should pass");
        Assert(phoneValidator.Validate("+44 20 7946 0958").IsValid, "UK phone format should pass");
        Assert(!phoneValidator.Validate("+1-555-CALL-NOW").IsValid, "Phone with letters should fail");
        
        Console.WriteLine("✓ Special characters and Unicode tests passed\n");
    }
    
    /// <summary>
    /// Tests various null and empty value scenarios
    /// </summary>
    private static void TestNullAndEmptyScenarios()
    {
        Console.WriteLine("Testing Null and Empty Scenarios:");
        
        // Arrange: String validators with different empty handling
        var allowEmptyValidator = Schema.String().MinLength(0);
        var notEmptyValidator = Schema.String().NotEmpty();
        
        // Act & Assert: Empty string handling
        Assert(allowEmptyValidator.Validate("").IsValid, "Empty string should pass when allowed");
        Assert(!notEmptyValidator.Validate("").IsValid, "Empty string should fail when not allowed");
        Assert(!allowEmptyValidator.Validate(null).IsValid, "Null string should fail even when empty allowed");
        Assert(!notEmptyValidator.Validate(null).IsValid, "Null string should fail when not empty required");
        
        // Arrange: Array with null items
        var arrayValidator = Schema.Array(Schema.String().Optional());
        
        // Act & Assert
        Assert(!arrayValidator.Validate(null).IsValid, "Null array should fail");
        Assert(arrayValidator.Validate(new string[0]).IsValid, "Empty array should pass");
        
        // Arrange: Object with null properties
        var userValidator = Schema.Object<User>()
            .Property(u => u.Id, Schema.String().NotEmpty())
            .Property(u => u.Name, Schema.String().NotEmpty())
            .Property(u => u.Email, Schema.String().NotEmpty())
            .Property(u => u.Age, Schema.Integer().Optional())
            .Property(u => u.IsActive, Schema.Boolean())
            .Property(u => u.Tags, Schema.Array(Schema.String()))
            .Property(u => u.Address, Schema.Object<Address>().Optional());
        
        // Act & Assert: User with null optional fields
        var userWithNulls = new User("123", "John", "john@test.com", null, true, 
            new List<string>(), null, null);
        Assert(userWithNulls != null && userValidator.Validate(userWithNulls).IsValid, "User with null optional fields should pass");
        
        // Arrange: Whitespace-only strings
        var trimmedValidator = Schema.String().NotEmpty();
        
        // Act & Assert
        Assert(!trimmedValidator.Validate("   ").IsValid, "Whitespace-only string should fail NotEmpty");
        Assert(!trimmedValidator.Validate("\t\n\r").IsValid, "Whitespace chars should fail NotEmpty");
        
        Console.WriteLine("✓ Null and empty scenario tests passed\n");
    }
    
    /// <summary>
    /// Tests validation path construction for nested objects
    /// </summary>
    private static void TestValidationPaths()
    {
        Console.WriteLine("Testing Validation Paths:");
        
        // Arrange: Deeply nested object structure
        var addressValidator = Schema.Object<Address>()
            .Property(a => a.Street, Schema.String().NotEmpty())
            .Property(a => a.PostalCode, Schema.String().Pattern(@"^\d{5}$"));
        
        var userValidator = Schema.Object<User>()
            .Property(u => u.Name, Schema.String().MinLength(2))
            .Property(u => u.Tags, Schema.Array(Schema.String().NotEmpty()))
            .Property(u => u.Address, addressValidator);
        
        // Arrange: Invalid nested object
        var invalidUser = new User("1", "J", "test@test.com", null, true,
            new List<string> { "valid", "", "alsovalid" },
            new Address("", "City", "ABC", "Country"), null);
        
        // Act
        var result = userValidator.Validate(invalidUser);
        
        // Assert: Check specific error paths
        Assert(!result.IsValid, "Validation should fail");
        Assert(result.Errors.Any(e => e.Path == "Name"), "Should have Name error");
        Assert(result.Errors.Any(e => e.Path == "Tags[1]"), "Should have Tags[1] error");
        Assert(result.Errors.Any(e => e.Path == "Address.Street"), "Should have Address.Street error");
        Assert(result.Errors.Any(e => e.Path == "Address.PostalCode"), "Should have Address.PostalCode error");
        
        // Arrange: Array of objects validation
        var arrayOfUsersValidator = Schema.Array(userValidator);
        var users = new[]
        {
            new User("1", "Valid User", "valid@test.com", null, true, new List<string> { "tag" }, 
                new Address("123 St", "City", "12345", "Country"), null),
            invalidUser
        };
        
        // Act
        var arrayResult = arrayOfUsersValidator.Validate(users);
        
        // Assert: Check array path construction
        Assert(!arrayResult.IsValid, "Array validation should fail");
        Assert(arrayResult.Errors.Any(e => e.Path.StartsWith("[1].")), "Should have errors with [1]. prefix");
        
        Console.WriteLine("✓ Validation path tests passed\n");
    }
    
    /// <summary>
    /// Tests combinations of different validators
    /// </summary>
    private static void TestValidatorCombinations()
    {
        Console.WriteLine("Testing Validator Combinations:");
        
        // Arrange: Complex chained string validator
        var complexStringValidator = Schema.String()
            .NotEmpty()
            .MinLength(8)
            .MaxLength(50)
            .Pattern(@"^[A-Za-z0-9@._-]+$")
            .WithMessage("Username must be 8-50 chars, alphanumeric with @._- allowed");
        
        // Act & Assert
        Assert(complexStringValidator.Validate("user123@test.com").IsValid, "Valid complex string should pass");
        Assert(!complexStringValidator.Validate("usr").IsValid, "Too short should fail");
        Assert(!complexStringValidator.Validate("user with spaces").IsValid, "Spaces should fail pattern");
        Assert(!complexStringValidator.Validate("user#with#hash").IsValid, "Hash symbols should fail pattern");
        
        // Arrange: Array of validated objects
        var tagValidator = Schema.String().MinLength(1).MaxLength(20).Pattern(@"^[a-z]+$");
        var userTagsValidator = Schema.Array(tagValidator).MinLength(1).MaxLength(5);
        
        // Act & Assert
        Assert(userTagsValidator.Validate(new[] { "admin", "user", "vip" }).IsValid, "Valid tags should pass");
        Assert(!userTagsValidator.Validate(new string[0]).IsValid, "Empty tags should fail (min 1)");
        Assert(!userTagsValidator.Validate(new[] { "admin", "USER" }).IsValid, "Uppercase tag should fail");
        Assert(!userTagsValidator.Validate(new[] { "a", "b", "c", "d", "e", "f" }).IsValid, "Too many tags should fail");
        
        // Arrange: Optional chained validator
        var optionalEmailValidator = Schema.String()
            .Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
            .MinLength(5)
            .MaxLength(100)
            .Optional();
        
        // Act & Assert
        Assert(optionalEmailValidator.Validate(null).IsValid, "Null optional email should pass");
        Assert(optionalEmailValidator.Validate("test@example.com").IsValid, "Valid optional email should pass");
        Assert(!optionalEmailValidator.Validate("invalid").IsValid, "Invalid optional email should fail");
        
        Console.WriteLine("✓ Validator combination tests passed\n");
    }
    
    /// <summary>
    /// Tests error accumulation and reporting
    /// </summary>
    private static void TestErrorAccumulation()
    {
        Console.WriteLine("Testing Error Accumulation:");
        
        // Arrange: Validator that can produce multiple errors
        var multiErrorValidator = Schema.Custom<string>((value, path) =>
        {
            var errors = new List<ValidationError>();
            
            if (value is null)
                errors.Add(new ValidationError(path, "Value cannot be null"));
            else
            {
                if (value.Length < 5)
                    errors.Add(new ValidationError(path, "Must be at least 5 characters"));
                if (!value.Any(char.IsUpper))
                    errors.Add(new ValidationError(path, "Must contain uppercase letter"));
                if (!value.Any(char.IsDigit))
                    errors.Add(new ValidationError(path, "Must contain digit"));
                if (!value.Any(c => "!@#$%^&*".Contains(c)))
                    errors.Add(new ValidationError(path, "Must contain special character"));
            }
            
            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        });
        
        // Act
        var result = multiErrorValidator.Validate("abc");
        
        // Assert: Multiple errors should be accumulated
        Assert(!result.IsValid, "Validation should fail");
        Assert(result.Errors.Count >= 3, "Should have at least 3 errors");
        Assert(result.Errors.Any(e => e.Message.Contains("5 characters")), "Should have length error");
        Assert(result.Errors.Any(e => e.Message.Contains("uppercase")), "Should have uppercase error");
        Assert(result.Errors.Any(e => e.Message.Contains("digit")), "Should have digit error");
        
        // Arrange: Object with multiple failing properties
        var strictUserValidator = Schema.Object<User>()
            .Property(u => u.Id, Schema.String().Pattern(@"^\d+$"))
            .Property(u => u.Name, Schema.String().MinLength(10))
            .Property(u => u.Email, Schema.String().Pattern(@"^[a-z]+@[a-z]+\.com$"))
            .Property(u => u.Age, Schema.Integer().Min(18).Max(65))
            .Property(u => u.Tags, Schema.Array(Schema.String().MinLength(5)).MinLength(2));
        
        var badUser = new User("abc", "Jo", "INVALID@TEST.ORG", 150, true,
            new List<string> { "x" }, null, null);
        
        // Act
        var userResult = strictUserValidator.Validate(badUser);
        
        // Assert: All property errors should be accumulated
        Assert(!userResult.IsValid, "User validation should fail");
        Assert(userResult.Errors.Count >= 5, "Should have errors for multiple properties");
        
        Console.WriteLine("✓ Error accumulation tests passed\n");
    }
    
    /// <summary>
    /// Tests thread safety of validators
    /// </summary>
    private static void TestThreadSafety()
    {
        Console.WriteLine("Testing Thread Safety:");
        
        // Arrange: Shared validator instance
        var sharedValidator = Schema.String().MinLength(3).MaxLength(10);
        var results = new System.Collections.Concurrent.ConcurrentBag<bool>();
        var tasks = new List<Task>();
        
        // Act: Run validation on multiple threads
        for (int i = 0; i < 10; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var testValue = $"test{taskId}{j}";
                    var result = sharedValidator.Validate(testValue);
                    results.Add(result.IsValid);
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert: All validations should succeed (thread-safe)
        Assert(results.Count == 1000, "Should have 1000 results");
        Assert(results.All(r => r), "All validations should succeed");
        
        // Arrange: Test object validator thread safety
        var sharedObjectValidator = ExampleValidators.CreateUserValidator();
        var objectResults = new System.Collections.Concurrent.ConcurrentBag<bool>();
        var objectTasks = new List<Task>();
        
        // Act: Validate objects on multiple threads
        for (int i = 0; i < 5; i++)
        {
            var taskId = i;
            objectTasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 50; j++)
                {
                    var user = new User($"user{taskId}{j}", "Valid User", $"user{taskId}{j}@test.com",
                        25, true, new List<string> { "tag" }, null, null);
                    var result = sharedObjectValidator.Validate(user);
                    objectResults.Add(result.IsValid);
                }
            }));
        }
        
        Task.WaitAll(objectTasks.ToArray());
        
        // Assert
        Assert(objectResults.Count == 250, "Should have 250 object validation results");
        Assert(objectResults.All(r => r), "All object validations should succeed");
        
        Console.WriteLine("✓ Thread safety tests passed\n");
    }
    
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Test failed: {message}");
        }
    }
}

/// <summary>
/// Performance tests to ensure the framework is efficient
/// </summary>
public static class PerformanceTests
{
    public static void RunPerformanceTests()
    {
        Console.WriteLine("=== Running Performance Tests ===\n");
        
        TestValidationPerformance();
        TestMemoryUsage();
        
        Console.WriteLine("=== Performance Tests Completed ===\n");
    }
    
    private static void TestValidationPerformance()
    {
        Console.WriteLine("Testing Validation Performance:");
        
        // Arrange
        var userValidator = ExampleValidators.CreateUserValidator();
        var testUser = new User("123", "John Doe", "john@example.com", 30, true,
            new List<string> { "tag1", "tag2" },
            new Address("123 Main St", "City", "12345", "USA"),
            new Dictionary<string, object> { { "key", "value" } });
        
        var iterations = 10000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Act
        for (int i = 0; i < iterations; i++)
        {
            userValidator.Validate(testUser);
        }
        
        stopwatch.Stop();
        
        // Assert
        var avgTimeMs = stopwatch.ElapsedMilliseconds / (double)iterations;
        Console.WriteLine($"Average validation time: {avgTimeMs:F4} ms per validation");
        Console.WriteLine($"Validations per second: {1000 / avgTimeMs:F0}");
        
        // Simple assertion that performance is reasonable
        if (avgTimeMs > 1.0) // More than 1ms per validation might indicate performance issues
        {
            Console.WriteLine("⚠️  Warning: Validation performance may be slower than expected");
        }
        else
        {
            Console.WriteLine("✓ Validation performance is acceptable");
        }
        
        Console.WriteLine();
    }
    
    private static void TestMemoryUsage()
    {
        Console.WriteLine("Testing Memory Usage:");
        
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        
        // Act
        // Create many validators to test memory usage
        var validators = new List<IValidator<string>>();
        for (int i = 0; i < 1000; i++)
        {
            validators.Add(Schema.String().MinLength(i % 10).MaxLength(i % 50 + 10));
        }
        
        // Assert
        var afterCreationMemory = GC.GetTotalMemory(false);
        var memoryUsed = afterCreationMemory - initialMemory;
        
        Console.WriteLine($"Memory used by 1000 validators: {memoryUsed / 1024.0:F2} KB");
        Console.WriteLine($"Average memory per validator: {memoryUsed / 1000.0:F2} bytes");
        
        // Clean up
        validators.Clear();
        GC.Collect();
        
        Console.WriteLine("✓ Memory usage test completed\n");
    }
}

/// <summary>
/// Integration tests showing real-world usage scenarios
/// </summary>
public static class IntegrationTests
{
    public static void RunIntegrationTests()
    {
        Console.WriteLine("=== Running Integration Tests ===\n");
        
        TestWebApiScenario();
        TestConfigurationValidation();
        TestBatchValidation();
        
        Console.WriteLine("=== Integration Tests Completed ===\n");
    }
    
    private static void TestWebApiScenario()
    {
        Console.WriteLine("Testing Web API Scenario:");
        
        // Arrange: Simulate validating incoming API requests
        var apiValidator = Schema.Object<User>()
            .Property(u => u.Id, Schema.String().NotEmpty().WithMessage("User ID is required"))
            .Property(u => u.Email, Schema.String()
                .Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
                .WithMessage("Please provide a valid email address"))
            .Property(u => u.Name, Schema.String()
                .MinLength(2)
                .MaxLength(100)
                .WithMessage("Name must be between 2 and 100 characters"));
        
        // Arrange: Test valid API request
        var validRequest = new User("user123", "John Doe", "john@example.com", null, true,
            new List<string>(), null, null);
        
        // Act
        var validResult = apiValidator.Validate(validRequest);
        
        // Assert
        Console.WriteLine($"Valid API request: {validResult.IsValid}");
        
        // Arrange: Test invalid API request
        var invalidRequest = new User("", "J", "invalid-email", null, true,
            new List<string>(), null, null);
        
        // Act
        var invalidResult = apiValidator.Validate(invalidRequest);
        
        // Assert
        Console.WriteLine($"Invalid API request: {invalidResult.IsValid}");
        Console.WriteLine("API validation errors:");
        foreach (var error in invalidResult.Errors)
        {
            Console.WriteLine($"  - {error.Path}: {error.Message}");
        }
        
        Console.WriteLine();
    }
    
    private static void TestConfigurationValidation()
    {
        Console.WriteLine("Testing Configuration Validation:");
        
        // Arrange: Example: Validating application configuration
        var configValidator = Schema.Custom<Dictionary<string, object>>((config, path) =>
        {
            var errors = new List<ValidationError>();
            
            if (!config.ContainsKey("ConnectionString"))
                errors.Add(new ValidationError($"{path}.ConnectionString", "Connection string is required"));
            
            if (config.TryGetValue("MaxRetries", out var retriesObj) && retriesObj is int retries)
            {
                if (retries < 0 || retries > 10)
                    errors.Add(new ValidationError($"{path}.MaxRetries", "Max retries must be between 0 and 10"));
            }
            
            if (config.TryGetValue("Timeout", out var timeoutObj) && timeoutObj is int timeout)
            {
                if (timeout <= 0)
                    errors.Add(new ValidationError($"{path}.Timeout", "Timeout must be positive"));
            }
            
            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        });
        
        var validConfig = new Dictionary<string, object>
        {
            { "ConnectionString", "Server=localhost;Database=test;" },
            { "MaxRetries", 3 },
            { "Timeout", 30 }
        };
        
        var invalidConfig = new Dictionary<string, object>
        {
            { "MaxRetries", 15 }, // Invalid: too high
            { "Timeout", -5 } // Invalid: negative
            // Missing ConnectionString
        };
        
        // Act & Assert
        Console.WriteLine($"Valid config: {configValidator.Validate(validConfig).IsValid}");
        
        var invalidResult = configValidator.Validate(invalidConfig);
        Console.WriteLine($"Invalid config: {invalidResult.IsValid}");
        foreach (var error in invalidResult.Errors)
        {
            Console.WriteLine($"  - {error.Message}");
        }
        
        Console.WriteLine();
    }
    
    private static void TestBatchValidation()
    {
        Console.WriteLine("Testing Batch Validation:");
        
        // Arrange
        var userValidator = ExampleValidators.CreateUserValidator();
        
        var users = new[]
        {
            new User("1", "Valid User", "valid@example.com", 25, true, new List<string> { "tag" }, null, null),
            new User("", "Invalid User", "invalid-email", -5, true, new List<string>(), null, null), // Invalid
            new User("3", "Another Valid", "valid2@example.com", 30, false, new List<string> { "tag" }, null, null)
        };
        
        // Act
        var results = users.Select(user => new { User = user, Result = userValidator.Validate(user) }).ToList();
        
        // Assert
        var validUsers = results.Where(r => r.Result.IsValid).ToList();
        var invalidUsers = results.Where(r => !r.Result.IsValid).ToList();
        
        Console.WriteLine($"Batch validation results: {validUsers.Count} valid, {invalidUsers.Count} invalid");
        
        foreach (var invalid in invalidUsers)
        {
            Console.WriteLine($"Invalid user {invalid.User.Name}:");
            foreach (var error in invalid.Result.Errors)
            {
                Console.WriteLine($"  - {error.Path}: {error.Message}");
            }
        }
        
        Console.WriteLine();
    }
} 