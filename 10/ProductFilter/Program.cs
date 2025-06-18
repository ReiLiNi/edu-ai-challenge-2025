using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace ProductFilter;

public class Product
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("rating")]
    public decimal Rating { get; set; }
    
    [JsonPropertyName("in_stock")]
    public bool InStock { get; set; }
}

public class OpenAIConfig
{
    public string ApiKey { get; set; } = string.Empty;
}

public class OpenAIProductFilter
{
    private readonly OpenAIClient _client;
    private readonly List<Product> _products;
    
    public OpenAIProductFilter(string apiKey, List<Product> products)
    {
        _client = new OpenAIClient(apiKey);
        _products = products;
    }
    
    public async Task<List<Product>> FilterProductsAsync(string userQuery)
    {
        var chatClient = _client.GetChatClient("gpt-4.1-mini");
        
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(@"You are a product filtering assistant. When the user asks to filter products, call the filter_products function with the complete products dataset and the user's filtering criteria.

The function should process the products and return only those that match the user's request.

Available categories: Electronics, Fitness, Kitchen, Books, Clothing
Be flexible with natural language filtering."),
            new UserChatMessage($"Filter products for: {userQuery}")
        };
        
        var chatCompletionOptions = new ChatCompletionOptions
        {
            Tools = {
                ChatTool.CreateFunctionTool(
                    functionName: "filter_products",
                    functionDescription: "Filter products based on user criteria and return matching products",
                    functionParameters: BinaryData.FromString($$$"""
                    {
                        "type": "object",
                        "properties": {
                            "products": {
                                "type": "array",
                                "description": "Complete dataset of products to filter",
                                "items": {
                                    "type": "object",
                                    "properties": {
                                        "name": {"type": "string"},
                                        "category": {"type": "string"},
                                        "price": {"type": "number"},
                                        "rating": {"type": "number"},
                                        "in_stock": {"type": "boolean"}
                                    }
                                }
                            },
                            "filter_criteria": {
                                "type": "string",
                                "description": "User's filtering criteria in natural language"
                            }
                        },
                        "required": ["products", "filter_criteria"]
                    }
                    """)
                )
            }
        };
        
        var response = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
        
        if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            var toolCall = response.Value.ToolCalls.FirstOrDefault();
            if (toolCall?.FunctionName == "filter_products")
            {
                // OpenAI is calling filter_products function - processing dataset
                
                // Create the function call result
                var functionResult = JsonSerializer.Serialize(new
                {
                    status = "ready_to_filter",
                    total_products = _products.Count,
                    query = userQuery
                });
                
                var functionMessage = new ToolChatMessage(toolCall.Id, functionResult);
                
                // Continue conversation with function result
                var newMessages = new List<ChatMessage>(messages);
                
                // Add assistant message with tool call
                var assistantMessage = new AssistantChatMessage("");
                assistantMessage.ToolCalls.Add(toolCall);
                newMessages.Add(assistantMessage);
                
                // Add function result
                newMessages.Add(functionMessage);
                
                // Add follow-up request with specific formatting instructions
                newMessages.Add(new UserChatMessage($@"Filter the products for: {userQuery}

From this dataset: {JsonSerializer.Serialize(_products, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })}

Return ONLY the matching products as a JSON array with no introduction, explanation, or additional text. Just the JSON array of matching products."));
                
                var finalResponse = await chatClient.CompleteChatAsync(newMessages);
                var responseText = finalResponse.Value.Content.FirstOrDefault()?.Text ?? "";
                
                // Parse OpenAI's filtered results
                var filteredProducts = ParseFilteredProducts(responseText);
                
                return filteredProducts;
            }
        }
        
                return new List<Product>();
    }
    
    private List<Product> ParseFilteredProducts(string responseText)
    {
        try
        {
            // Clean the response text
            responseText = responseText.Trim();
            
            // Try to find JSON array markers
            var jsonStart = responseText.IndexOf('[');
            var jsonEnd = responseText.LastIndexOf(']');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                
                // Try parsing as JSON
                var filteredProducts = JsonSerializer.Deserialize<List<Product>>(jsonText, 
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                
                return filteredProducts ?? new List<Product>();
            }
            
            // If no JSON found, try to extract product information from text
            return ExtractProductsFromText(responseText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing OpenAI response: {ex.Message}");
            return new List<Product>();
        }
    }
    
    private List<Product> ExtractProductsFromText(string text)
    {
        var products = new List<Product>();
        
        // Split by lines and look for product patterns
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip lines that don't contain product information
            if (string.IsNullOrEmpty(trimmedLine) || 
                trimmedLine.StartsWith("Here") || 
                trimmedLine.StartsWith("Based") ||
                trimmedLine.Contains("dataset") ||
                !trimmedLine.Contains("$"))
                continue;
            
            // Try to find matching products from our dataset by name patterns
            foreach (var product in _products)
            {
                if (trimmedLine.ToLower().Contains(product.Name.ToLower()) ||
                    product.Name.ToLower().Contains(trimmedLine.ToLower().Split(' ')[0]))
                {
                    if (!products.Any(p => p.Name == product.Name))
                    {
                        products.Add(product);
                    }
                }
            }
        }
        
        return products;
    }
 
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Product Filter Console App");
        Console.WriteLine("==========================");
        
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        var openAIConfig = configuration.GetSection("OpenAI").Get<OpenAIConfig>();
        
        if (openAIConfig == null || string.IsNullOrEmpty(openAIConfig.ApiKey) || openAIConfig.ApiKey == "your-openai-api-key-here")
        {
            Console.WriteLine("Error: Please set your OpenAI API key in appsettings.json");
            Console.WriteLine("Update the 'OpenAI:ApiKey' value in appsettings.json with your actual API key.");
            return;
        }
        
        // Load products from JSON file
        var productsJson = await File.ReadAllTextAsync("products.json");
        var products = JsonSerializer.Deserialize<List<Product>>(productsJson) ?? new List<Product>();
        
        Console.WriteLine($"Loaded {products.Count} products from products.json");
        Console.WriteLine($"Using OpenAI API key from configuration");
        Console.WriteLine("Products dataset will be passed to OpenAI for direct filtering");
        
        var openAIFilter = new OpenAIProductFilter(openAIConfig.ApiKey, products);
        
        while (true)
        {
            Console.WriteLine("\nEnter your product filter request (or 'quit' to exit):");
            Console.Write("> ");
            var userInput = Console.ReadLine();
            
            if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "quit")
                break;
            
            try
            {
                Console.WriteLine("Processing your request with OpenAI...");
                var filteredProducts = await openAIFilter.FilterProductsAsync(userInput);
                
                var result = FormatResults(filteredProducts);
                
                // Output only the exact formatted result without any extra text
                Console.WriteLine();
                Console.WriteLine(result);
                Console.WriteLine();
                
                // Save to file
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"filtered_products_{timestamp}.txt";
                await File.WriteAllTextAsync(fileName, result);
                Console.WriteLine($"Results saved to: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    
    static string FormatResults(List<Product> products)
    {
        if (!products.Any())
            return "No products found matching your criteria.";
        
        var result = "Filtered Products:\n";
        for (int i = 0; i < products.Count; i++)
        {
            var product = products[i];
            var stockStatus = product.InStock ? "In Stock" : "Out of Stock";
            result += $"{i + 1}. {product.Name} - ${product.Price:F2}, Rating: {product.Rating}, {stockStatus}\n";
        }
        
        return result.TrimEnd('\n'); // Remove trailing newline for exact formatting
    }
}