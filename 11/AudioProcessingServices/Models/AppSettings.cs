namespace AudioProcessingServices.Models
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string WhisperModel { get; set; } = "whisper-1";
        public string GptModel { get; set; } = "gpt-4";
        public int MaxTokens { get; set; } = 2000;
        public double Temperature { get; set; } = 0.3;
    }

    public class TranscriptionSettings
    {
        public string OutputDirectory { get; set; } = "Transcriptions";
        public string FilePrefix { get; set; } = "transcript_";
        public string DateFormat { get; set; } = "yyyyMMdd_HHmmss";
    }

    public class AnalyticsSettings
    {
        public string TopicExtractionPrompt { get; set; } = string.Empty;
        public string SummaryPrompt { get; set; } = string.Empty;
    }
} 