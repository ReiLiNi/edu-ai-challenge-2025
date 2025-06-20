using AudioProcessingServices.Interfaces;
using AudioProcessingServices.Models;
using Microsoft.Extensions.Logging;

namespace AudioProcessingServices.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private readonly IAudioTranscriptionService _transcriptionService;
        private readonly ITextAnalysisService _textAnalysisService;
        private readonly IFileService _fileService;
        private readonly ILogger<AudioProcessingService> _logger;

        public AudioProcessingService(
            IAudioTranscriptionService transcriptionService,
            ITextAnalysisService textAnalysisService,
            IFileService fileService,
            ILogger<AudioProcessingService> logger)
        {
            _transcriptionService = transcriptionService;
            _textAnalysisService = textAnalysisService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<TranscriptionResult> ProcessAudioAsync(string audioFilePath)
        {
            try
            {
                _logger.LogInformation("Starting audio processing for file: {AudioFile}", audioFilePath);

                if (!File.Exists(audioFilePath))
                {
                    throw new FileNotFoundException($"Audio file not found: {audioFilePath}");
                }

                // Step 1: Get audio duration
                var audioDuration = await _transcriptionService.GetAudioDurationAsync(audioFilePath);

                // Step 2: Transcribe audio
                var transcriptionText = await _transcriptionService.TranscribeAudioAsync(audioFilePath);

                // Step 3: Calculate basic analytics
                var analytics = _textAnalysisService.CalculateAnalytics(transcriptionText, audioDuration);

                // Step 4: Extract topics (in parallel with summarization)
                var topicsTask = _textAnalysisService.ExtractTopicsAsync(transcriptionText);
                var summaryTask = _textAnalysisService.SummarizeTextAsync(transcriptionText);

                await Task.WhenAll(topicsTask, summaryTask);

                var topics = await topicsTask;
                var summary = await summaryTask;

                // Step 5: Combine results
                analytics.FrequentlyMentionedTopics = topics;

                var result = new TranscriptionResult
                {
                    Text = transcriptionText,
                    Summary = summary,
                    Analytics = analytics,
                    AudioFileName = Path.GetFileName(audioFilePath),
                    AudioDurationSeconds = audioDuration,
                    CreatedAt = DateTime.UtcNow
                };

                // Step 6: Save transcription to file
                var savedFilePath = await _fileService.SaveTranscriptionAsync(result);
                
                _logger.LogInformation("Audio processing completed successfully. Results saved to: {SavedFile}", savedFilePath);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audio processing for file: {AudioFile}", audioFilePath);
                throw;
            }
        }
    }
} 