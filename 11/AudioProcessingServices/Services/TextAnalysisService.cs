using AudioProcessingServices.Interfaces;
using AudioProcessingServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AudioProcessingServices.Services
{
    public class TextAnalysisService : ITextAnalysisService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly OpenAISettings _openAISettings;
        private readonly AnalyticsSettings _analyticsSettings;
        private readonly ILogger<TextAnalysisService> _logger;

        public TextAnalysisService(
            IConfiguration configuration,
            ILogger<TextAnalysisService> logger)
        {
            _openAISettings = configuration.GetSection("OpenAI").Get<OpenAISettings>() ?? new OpenAISettings();
            _analyticsSettings = configuration.GetSection("Analytics").Get<AnalyticsSettings>() ?? new AnalyticsSettings();
            _openAIClient = new OpenAIClient(_openAISettings.ApiKey);
            _logger = logger;
        }

        public async Task<string> SummarizeTextAsync(string text)
        {
            try
            {
                _logger.LogInformation("Starting text summarization. Text length: {TextLength}", text.Length);

                var chatClient = _openAIClient.GetChatClient(_openAISettings.GptModel);
                
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(_analyticsSettings.SummaryPrompt),
                    new UserChatMessage(text)
                };

                var chatOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = _openAISettings.MaxTokens,
                    Temperature = (float)_openAISettings.Temperature
                };

                var response = await chatClient.CompleteChatAsync(messages, chatOptions);
                var summary = response.Value.Content[0].Text;

                _logger.LogInformation("Text summarization completed successfully");
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during text summarization");
                throw;
            }
        }

        public async Task<List<TopicMention>> ExtractTopicsAsync(string text)
        {
            try
            {
                _logger.LogInformation("Starting topic extraction");

                var chatClient = _openAIClient.GetChatClient(_openAISettings.GptModel);
                
                var topicPrompt = $@"{_analyticsSettings.TopicExtractionPrompt}
                
Please return the response in the following JSON format:
[
  {{""topic"": ""Topic Name"", ""mentions"": 5}},
  {{""topic"": ""Another Topic"", ""mentions"": 3}}
]

Transcript to analyze:
{text}";

                var messages = new List<ChatMessage>
                {
                    new UserChatMessage(topicPrompt)
                };

                var chatOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 1000,
                    Temperature = 0.1f
                };

                var response = await chatClient.CompleteChatAsync(messages, chatOptions);
                var topicsJson = response.Value.Content[0].Text;

                // Try to extract JSON from the response
                var jsonMatch = Regex.Match(topicsJson, @"\[.*\]", RegexOptions.Singleline);
                if (jsonMatch.Success)
                {
                    var topics = JsonConvert.DeserializeObject<List<TopicMention>>(jsonMatch.Value);
                    _logger.LogInformation("Successfully extracted {TopicCount} topics", topics?.Count ?? 0);
                    return topics ?? new List<TopicMention>();
                }

                _logger.LogWarning("Could not parse topics JSON response: {Response}", topicsJson);
                return new List<TopicMention>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during topic extraction");
                return new List<TopicMention>();
            }
        }

        public TranscriptionAnalytics CalculateAnalytics(string text, double audioDurationSeconds)
        {
            try
            {
                _logger.LogInformation("Calculating text analytics");

                // Calculate word count
                var words = Regex.Matches(text, @"\b\w+\b").Count;

                // Calculate speaking speed (words per minute)
                var durationMinutes = audioDurationSeconds / 60.0;
                var wordsPerMinute = durationMinutes > 0 ? (int)Math.Round(words / durationMinutes) : 0;

                var analytics = new TranscriptionAnalytics
                {
                    WordCount = words,
                    SpeakingSpeedWpm = wordsPerMinute,
                    FrequentlyMentionedTopics = new List<TopicMention>() // Will be populated separately
                };

                _logger.LogInformation("Analytics calculated - Words: {WordCount}, WPM: {WPM}", words, wordsPerMinute);
                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating analytics");
                return new TranscriptionAnalytics();
            }
        }
    }
} 