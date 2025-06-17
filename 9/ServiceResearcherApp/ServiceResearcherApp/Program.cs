using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text;

namespace ServiceResearcher;

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static IConfiguration? configuration;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Service Researcher ===");
        Console.WriteLine("This tool researches well-known services and generates structured reports.");
        Console.WriteLine();

        // Load configuration
        LoadConfiguration();

        // Get OpenAI API Key
        string? apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Please set your OpenAI API key in appsettings.json or as environment variable OPENAI_API_KEY");
            return;
        }

        // Get service name from user
        string serviceName = GetServiceNameFromUser();
        if (string.IsNullOrEmpty(serviceName))
        {
            Console.WriteLine("No service name provided. Exiting...");
            return;
        }

        Console.WriteLine($"\nResearching '{serviceName}'...");

        try
        {
            // Create OpenAI client using the newer API
            ChatClient client = new(model: "gpt-4.1-mini", apiKey: apiKey);

            // Generate research prompt
            string prompt = GenerateResearchPrompt(serviceName);

            // Call OpenAI API
            ChatCompletion completion = await client.CompleteChatAsync(prompt);
            string research = completion.Content[0].Text;

            // Generate output filename
            string fileName = GenerateFileName(serviceName);

            // Save to markdown file
            await SaveToMarkdownFile(fileName, serviceName, research);

            Console.WriteLine($"\nResearch completed! Results saved to: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
        }
    }

    private static void LoadConfiguration()
    {
        // Get the current environment (defaults to Development for easier debugging)
        string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                           ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                           ?? "Development";

        // Debug information
        Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"Configuration environment: {environment}");

        // Try multiple base paths to find configuration files
        string[] possibleBasePaths = {
            Directory.GetCurrentDirectory(),                           // Current directory
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", // Executable location
            AppDomain.CurrentDomain.BaseDirectory,                     // App base directory
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."), // VS debugging path
        };

        var builder = new ConfigurationBuilder();
        
        // Try each base path to find the configuration files
        string? foundBasePath = null;
        foreach (string basePath in possibleBasePaths)
        {
            string appsettingsPath = Path.Combine(basePath, "appsettings.json");
            if (File.Exists(appsettingsPath))
            {
                foundBasePath = basePath;
                Console.WriteLine($"Found configuration files in: {basePath}");
                break;
            }
        }

        if (foundBasePath != null)
        {
            builder.SetBasePath(foundBasePath)
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
        }
        else
        {
            Console.WriteLine("Warning: Could not find appsettings.json files. Using environment variables only.");
            builder.SetBasePath(Directory.GetCurrentDirectory());
        }

        builder.AddEnvironmentVariables();
        configuration = builder.Build();
        
        // Debug: Show what API key was loaded
        string? apiKeySource = "";
        string? loadedApiKey = configuration?["OpenAI:ApiKey"];
        if (!string.IsNullOrEmpty(loadedApiKey))
        {
            apiKeySource = "configuration file";
        }
        else
        {
            loadedApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrEmpty(loadedApiKey))
            {
                apiKeySource = "environment variable";
            }
        }
        
        if (!string.IsNullOrEmpty(loadedApiKey))
        {
            Console.WriteLine($"✓ API key loaded from {apiKeySource} (length: {loadedApiKey.Length})");
        }
        else
        {
            Console.WriteLine("✗ No API key found in configuration or environment variables");
        }
    }

    private static string? GetApiKey()
    {
        // Try to get from configuration file first
        string? apiKey = configuration?["OpenAI:ApiKey"];
        
        // If not found, try environment variable
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        return apiKey;
    }

    private static string GetServiceNameFromUser()
    {
        Console.Write("Enter the name of the service to research: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static string GenerateResearchPrompt(string serviceName)
    {
        return $@"Please provide comprehensive research about the service ""{serviceName}"". 
Structure your response in the following sections with clear headers:

## Brief History
- Founding year, key milestones, evolution over time

## Target Audience
- Primary user segments and demographics

## Core Features
- Top 2-4 key functionalities that define the service

## Unique Selling Points
- Key differentiators that set it apart from competitors

## Business Model
- How the service generates revenue

## Tech Stack Insights
- Technologies, platforms, or frameworks used (if publicly known)

## Perceived Strengths
- Commonly mentioned positive aspects and standout features

## Perceived Weaknesses
- Frequently cited limitations, drawbacks, or areas for improvement

Please provide factual, well-researched information. If certain information is not publicly available or uncertain, please note that explicitly.";
    }

    private static string GenerateFileName(string serviceName)
    {
        // Clean service name for filename
        string cleanName = serviceName.Replace(" ", "_")
                                    .Replace("/", "_")
                                    .Replace("\\", "_")
                                    .Replace(":", "")
                                    .Replace("*", "")
                                    .Replace("?", "")
                                    .Replace("\"", "")
                                    .Replace("<", "")
                                    .Replace(">", "")
                                    .Replace("|", "");

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"{cleanName}_{timestamp}.md";
    }

    private static async Task SaveToMarkdownFile(string fileName, string serviceName, string content)
    {
        var markdown = new StringBuilder();
        
        // Add title and metadata
        markdown.AppendLine($"# Service Research Report: {serviceName}");
        markdown.AppendLine();
        markdown.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        markdown.AppendLine($"**Service:** {serviceName}");
        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine();
        
        // Add the research content
        markdown.AppendLine(content);
        
        // Add footer
        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine("*Report generated by Service Researcher tool*");

        await File.WriteAllTextAsync(fileName, markdown.ToString());
    }
}
