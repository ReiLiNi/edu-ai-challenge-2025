# Service Researcher

A .NET Core console application that researches well-known services using OpenAI API and generates structured markdown reports.

## Project Structure

```
📂 ServiceResearcher/                  ← Solution Root
├── ServiceResearcher.sln              ← Visual Studio Solution File
├── README.md                          ← This file (also in Solution Items)
├── .gitignore                         ← Git exclusions (also in Solution Items)
└── ServiceResearcher/                 ← Project Directory
    ├── Program.cs                     ← Main application logic
    ├── ServiceResearcher.csproj       ← Project file
    ├── appsettings.json               ← Base configuration (Solution Item)
    └── appsettings.Development.json   ← Development configuration (Solution Item)
```

**Visual Studio Solution Explorer Structure:**
```
📁 Solution 'ServiceResearcher'
├── 📁 Solution Items
│   ├── 📄 README.md
│   ├── 📄 .gitignore
│   ├── 📄 appsettings.json
│   └── 📄 appsettings.Development.json
└── 📁 ServiceResearcher (Project)
    ├── 📄 Program.cs
    ├── 📄 ServiceResearcher.csproj
    ├── 📄 appsettings.json
    └── 📄 appsettings.Development.json
```

## Features

- Interactive console interface for service input
- OpenAI API integration for comprehensive service research
- Structured output with predefined sections:
  - Brief History
  - Target Audience
  - Core Features
  - Unique Selling Points
  - Business Model
  - Tech Stack Insights
  - Perceived Strengths
  - Perceived Weaknesses
- Automatic markdown file generation with timestamps

## Setup

### Prerequisites

- .NET 9.0 or later
- OpenAI API key

### Installation

1. **Open the project:**
   - **Option A (Visual Studio):** Double-click `ServiceResearcher.sln` to open in Visual Studio
   - **Option B (Command Line):** Navigate to the solution directory

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

### Configuration

You can provide your OpenAI API key in three ways:

#### Option 1: Development Configuration File (Recommended)
1. In Visual Studio, find `appsettings.Development.json` in the Solution Items folder or project
2. Edit the file and replace the placeholder with your actual OpenAI API key:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-your-actual-api-key-here"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.Extensions": "Warning"
       }
     }
   }
   ```
   **Note:** This file is automatically excluded from git commits to protect your API key.

#### Option 2: Environment Variable
Set the `OPENAI_API_KEY` environment variable:

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY="sk-your-actual-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=sk-your-actual-api-key-here
```

**Linux/macOS:**
```bash
export OPENAI_API_KEY=sk-your-actual-api-key-here
```

#### Option 3: Base Configuration File (Not Recommended)
Edit `ServiceResearcher/appsettings.json` directly, but be careful not to commit your API key to version control.

## Usage

### Using Visual Studio
1. Open `ServiceResearcher.sln` in Visual Studio
2. Set ServiceResearcher as the startup project (if not already)
3. Configure your API key in `appsettings.Development.json` (visible in Solution Items)
4. Press F5 or click "Start Debugging"

### Using Command Line
1. Build the solution:
   ```bash
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run --project ServiceResearcher
   ```

3. Enter the name of the service you want to research when prompted

4. Wait for the research to complete

5. The application will generate a markdown file with the research results

## Output Format

The generated markdown files include:
- Service name and generation timestamp
- Structured research sections as specified
- Professional formatting with headers and dividers

## Example Services to Research

- Netflix
- Spotify
- GitHub
- Slack
- Discord
- Zoom
- Dropbox
- Airbnb
- Uber

## Notes

- Files are saved with the format: `ServiceName_YYYYMMDD_HHMMSS.md`
- Special characters in service names are automatically cleaned for filenames
- The application uses GPT-4o-mini model for cost-effective research
- Research quality depends on publicly available information about the service
- `appsettings.Development.json` is excluded from git commits to protect your API key
- Configuration files are visible in Visual Studio Solution Explorer under "Solution Items"

## Troubleshooting

- **"Please set your OpenAI API key"**: Ensure your API key is properly configured in `appsettings.Development.json` or environment variables
- **Network errors**: Check your internet connection and API key validity
- **File permission errors**: Ensure the application has write permissions in the current directory
- **Configuration not loading**: Verify that `appsettings.Development.json` is in the correct directory and properly formatted
- **Files not visible in Visual Studio**: Check the Solution Items folder in Solution Explorer 