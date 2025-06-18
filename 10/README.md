# Product Filter Console App

A .NET console application that uses OpenAI's function calling API to filter products based on natural language input.

## Quick Start (Visual Studio)

### Prerequisites
- Visual Studio 2022 (or VS Code)
- .NET 8.0 SDK
- OpenAI API key

### Setup & Run

1. **Open in Visual Studio**
   - Double-click `ProductFilterSolution.sln`
   - Or use **File → Open → Project/Solution** in Visual Studio

2. **Configure API Key**
   - Open `ProductFilter/appsettings.json`
   - Replace `"your-openai-api-key-here"` with your actual OpenAI API key:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-your-actual-api-key-here"
     }
   }
   ```

3. **Run the Application**
   - Press **F5** (with debugging) or **Ctrl+F5** (without debugging)
   - Or right-click `ProductFilter` project → **Debug → Start New Instance**

### How to Use

1. Enter natural language queries when prompted:
   - `"Show me electronics under $100"`
   - `"Find fitness equipment with high ratings"`
   - `"Show me all books in stock"`

2. View results in exact format:
   ```
   Filtered Products:
   1. Wireless Headphones - $99.99, Rating: 4.5, In Stock
   2. Smart Watch - $199.99, Rating: 4.6, In Stock
   ```

3. Results are automatically saved to timestamped files

## Command Line Alternative

```bash
cd ProductFilter
dotnet run
```

## Features

- **OpenAI Function Calling**: Passes complete products dataset to OpenAI for intelligent filtering
- **Natural Language Processing**: Understands complex filtering requests
- **Exact Output Format**: Clean, structured results without GPT introductions
- **Auto-Save**: Results saved to timestamped files
- **50+ Products**: Electronics, Fitness, Kitchen, Books, Clothing categories

## Project Structure

```
ProductFilter/
├── ProductFilterSolution.sln    # Visual Studio solution
├── Program.cs                   # Main application
├── ProductFilter.csproj         # Project file
├── appsettings.json            # Configuration (API key)
├── products.json               # Products dataset
└── README.md                   # This file
```

## Troubleshooting

**Build Errors:**
- Ensure .NET 8.0 SDK is installed
- Restore NuGet packages: **Build → Restore NuGet Packages**

**API Errors:**
- Verify OpenAI API key in `appsettings.json`
- Check internet connection
- Ensure API key has GPT-4 access

**No Results:**
- Try simpler queries like `"show me electronics"`
- Check that `products.json` is in the project directory

## Technical Details

- **Framework**: .NET 8.0
- **OpenAI Model**: GPT-4.1-mini with function calling
- **Dependencies**: OpenAI SDK, Microsoft.Extensions.Configuration
- **Input**: Natural language queries
- **Output**: Filtered product lists in structured format 