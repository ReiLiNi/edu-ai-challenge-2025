namespace AudioProcessingServices.Models
{
    public class TranscriptionResult
    {
        public string Text { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public TranscriptionAnalytics Analytics { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AudioFileName { get; set; } = string.Empty;
        public double AudioDurationSeconds { get; set; }
    }

    public class TranscriptionAnalytics
    {
        public int WordCount { get; set; }
        public int SpeakingSpeedWpm { get; set; }
        public List<TopicMention> FrequentlyMentionedTopics { get; set; } = new();
    }

    public class TopicMention
    {
        public string Topic { get; set; } = string.Empty;
        public int Mentions { get; set; }
    }
} 