using AudioProcessingServices.Models;

namespace AudioProcessingServices.Interfaces
{
    public interface ITextAnalysisService
    {
        Task<string> SummarizeTextAsync(string text);
        Task<List<TopicMention>> ExtractTopicsAsync(string text);
        TranscriptionAnalytics CalculateAnalytics(string text, double audioDurationSeconds);
    }
} 