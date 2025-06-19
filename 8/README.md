# Validation Framework

This project is a robust, type-safe validation library for .NET, inspired by modern validation libraries like Zod. It provides a fluent, chainable API to build complex validation schemas for your data.

## Features

- **Type-Safe Validators**: Fluent APIs for primitive types (`string`, `number`, `boolean`, `date`).
- **Complex Type Support**: Validators for `Array` and `Object` types.
- **Extensible**: Easily create your own custom validators.
- **Optional Values**: Support for optional and nullable values.
- **Custom Error Messages**: Easily override default error messages.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (Optional)

### Running from the Command Line

#### Building the Solution

To build the entire solution, run the following command from the root directory:

```bash
dotnet build ValidationFramework.sln
```

### Running the Examples

The `ValidationFramework.Examples` project contains a console application demonstrating how to use the library. To run it, use the following command:

```bash
dotnet run --project ValidationFramework.Examples
```

### Running Tests

The solution includes a comprehensive test suite in the `ValidationFramework.Tests` project. To run the tests, use the following command:

```bash
dotnet test
```

### Generating a Coverage Report

To generate a code coverage report, run the following command. The report will be generated in the `CoverageReport` directory.

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### Running from Visual Studio

1.  **Open the Solution**: Open the `ValidationFramework.sln` file in Visual Studio 2022.
2.  **Run the Examples**:
    *   In the Solution Explorer, right-click on the `ValidationFramework.Examples` project and select "Set as Startup Project".
    *   Press `F5` or click the "Start" button to run the example application.
3.  **Run the Tests**:
    *   Go to the "Test" menu and open the "Test Explorer".
    *   Click the "Run All Tests" button to execute the entire test suite.

## How to Use

The library is designed to be intuitive and easy to use. Here's a quick example of how to define a validation schema:

```csharp
using ValidationFramework;
using System.Text.RegularExpressions;

// 1. Define a schema for an address
var addressSchema = new Dictionary<string, IValidator<object>>
{
    { "Street", Schema.String().MinLength(5).AsBase() },
    { "City", Schema.String().MinLength(2).AsBase() },
    { "PostalCode", Schema.String().Pattern(new Regex(@"^\d{5}$")).AsBase() }
};

// 2. Define a schema for a user
var userSchema = new Dictionary<string, IValidator<object>>
{
    { "Name", Schema.String().MinLength(2).AsBase() },
    { "Email", Schema.String().Email().AsBase() },
    { "Age", Schema.Number().Min(18).Optional().AsBase() },
    { "Address", Schema.Object(addressSchema).Optional().AsBase() }
};

// 3. Create a validator from the schema
var userValidator = Schema.Object(userSchema);

// 4. Validate your data
var userData = new Dictionary<string, object>
{
    { "Name", "John Doe" },
    { "Email", "john.doe@example.com" }
};

var result = userValidator.Validate(userData);

if (result.IsValid)
{
    Console.WriteLine("User data is valid!");
}
else
{
    Console.WriteLine("Validation failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
``` 