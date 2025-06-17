# Service Researcher App

A .NET 8 console application that researches well-known services and generates comprehensive structured reports using OpenAI's API.

## 🚀 Features

- **Automated Service Research**: Get detailed insights about any service or company
- **Structured Reports**: Clean, professional markdown reports with standardized sections
- **OpenAI Integration**: Powered by GPT models for accurate and comprehensive research
- **Environment-Specific Configuration**: Separate settings for Development and Production
- **Clean Output**: Automatically removes conversational AI responses for professional reports

## 📋 Prerequisites

- **.NET 8 SDK** or later
- **Visual Studio 2022** (recommended) or Visual Studio Code
- **OpenAI API Key** with appropriate model access

## ⚙️ Setup Instructions

### 1. Clone and Open Project

1. Open **Visual Studio 2022**
2. Open the solution file: `ServiceResearcherApp.sln`
3. The project should automatically restore NuGet packages

### 2. Configure API Key

**Option A: Development Configuration (Recommended for testing)**
1. Open `ServiceResearcherApp/appsettings.Development.json`
2. Add your OpenAI API key:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Extensions": "Warning"
    }
  }
}
```

**Option B: Environment Variable**
1. Set the environment variable `OPENAI_API_KEY` in your system
2. Or set `DOTNET_ENVIRONMENT=Development` to use the development configuration

**Option C: Production Configuration**
1. Open `ServiceResearcherApp/appsettings.json`
2. Add your API key (not recommended for source control)

### 3. Set Environment (Optional)

To use Development configuration by default:
1. Right-click the **ServiceResearcherApp** project in Solution Explorer
2. Select **Properties**
3. Go to **Debug** → **General**
4. Click **Open debug launch profiles UI**
5. Add environment variable: `DOTNET_ENVIRONMENT` = `Development`

## 🏃‍♂️ Running the Application

### From Visual Studio

1. **Set as Startup Project**: Right-click `ServiceResearcherApp` project → **Set as Startup Project**
2. **Run**: Press `F5` or click the **Start** button
3. **Follow prompts**: Enter the name of the service you want to research when prompted

### From Command Line

```bash
cd ServiceResearcherApp
dotnet run
```

### Sample Usage

```
=== Service Researcher ===
This tool researches well-known services and generates structured reports.

Configuration loaded for environment: Development
✓ API key loaded from configuration file (length: 164)
Enter the name of the service to research: Netflix

Researching 'Netflix'...
Research completed! Results saved to: Netflix_20250617_143022.md
```

## 📁 Generated Files Location

### When Running from Visual Studio
Reports are saved in the **project output directory**:
```
ServiceResearcherApp/bin/Debug/net8.0/
```

### When Running from Command Line
Reports are saved in the **project root directory**:
```
ServiceResearcherApp/
```

### File Naming Convention
```
{ServiceName}_{Timestamp}.md
```
Examples:
- `Netflix_20250617_143022.md`
- `Discord_20250617_144015.md`
- `GitHub_20250617_145300.md`

## 📊 Report Structure

Each generated report contains:

- **Brief History**: Founding, milestones, evolution
- **Target Audience**: User segments and demographics  
- **Core Features**: Key functionalities
- **Unique Selling Points**: Competitive differentiators
- **Business Model**: Revenue generation methods
- **Tech Stack Insights**: Technologies and platforms used
- **Perceived Strengths**: Positive aspects and standout features
- **Perceived Weaknesses**: Limitations and improvement areas

## 🔧 Configuration Details

### Supported Models
The app automatically tries multiple OpenAI models in order of preference:
- gpt-4.1-mini


### Configuration Loading Order
1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific overrides)
3. Environment variables (highest priority)

### Debug Information
The app displays helpful information when starting:
- Current working directory
- Configuration environment detected
- API key loading status
- Configuration files found

## 🏗️ Project Structure

```
ServiceResearcherApp/
├── ServiceResearcherApp/
│   ├── Program.cs              # Main application logic
│   ├── ServiceResearcherApp.csproj  # Project file with dependencies
│   ├── appsettings.json        # Base configuration
│   ├── appsettings.Development.json  # Development configuration
│   └── bin/Debug/net8.0/       # Output directory (generated files here)
├── ServiceResearcherApp.sln    # Solution file
└── README.md                   # This file
```

## 📦 Dependencies

- **OpenAI** (v2.1.0) - OpenAI API integration
- **Microsoft.Extensions.Configuration** (v8.0.0) - Configuration management
- **Microsoft.Extensions.Configuration.Json** (v8.0.1) - JSON configuration support
- **Microsoft.Extensions.Configuration.EnvironmentVariables** (v8.0.0) - Environment variable support

## 🎯 Tips for Best Results

1. **Use specific service names**: "Netflix" works better than "streaming service"
2. **Try well-known services**: Popular services have more available information
3. **Check generated reports**: Reports are comprehensive and ready for business use
4. **Archive reports**: Files include timestamps for easy organization

## 📝 License

This project is for educational and research purposes.

---
*Happy researching! 🔍* 