using AudioProcessingServices.Interfaces;
using AudioProcessingServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Audio;

namespace AudioProcessingServices.Services
{
    public class AudioTranscriptionService : IAudioTranscriptionService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly OpenAISettings _settings;
        private readonly ILogger<AudioTranscriptionService> _logger;

        public AudioTranscriptionService(
            IConfiguration configuration,
            ILogger<AudioTranscriptionService> logger)
        {
            _settings = configuration.GetSection("OpenAI").Get<OpenAISettings>() ?? new OpenAISettings();
            _openAIClient = new OpenAIClient(_settings.ApiKey);
            _logger = logger;
        }

        public async Task<string> TranscribeAudioAsync(string audioFilePath)
        {
            try
            {
                _logger.LogInformation("Starting transcription for file: {AudioFile}", audioFilePath);

                using var audioFile = File.OpenRead(audioFilePath);
                var fileName = Path.GetFileName(audioFilePath);

                var transcriptionOptions = new AudioTranscriptionOptions
                {
                    ResponseFormat = AudioTranscriptionFormat.Text,
                    Temperature = 0.0f
                };

                var audioClient = _openAIClient.GetAudioClient(_settings.WhisperModel);
                var response = await audioClient.TranscribeAudioAsync(audioFile, fileName, transcriptionOptions);

                var transcriptionText = response.Value.Text;
                
                _logger.LogInformation("Transcription completed successfully. Text length: {TextLength}", transcriptionText.Length);
                
                return transcriptionText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audio transcription for file: {AudioFile}", audioFilePath);
                throw;
            }
        }

        public async Task<double> GetAudioDurationAsync(string audioFilePath)
        {
            try
            {
                // For this implementation, we'll use a simplified approach
                // In a production environment, you might want to use a proper audio library
                // like NAudio or FFMpegCore to get accurate duration
                
                var fileInfo = new FileInfo(audioFilePath);
                // Rough estimation: MP3 files are typically around 1MB per minute at standard quality
                // This is a very rough estimate and should be replaced with proper audio duration detection
                var estimatedDurationMinutes = fileInfo.Length / (1024.0 * 1024.0); // MB
                var estimatedDurationSeconds = estimatedDurationMinutes * 60;
                
                _logger.LogInformation("Estimated audio duration: {Duration} seconds", estimatedDurationSeconds);
                
                return await Task.FromResult(estimatedDurationSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audio duration for file: {AudioFile}", audioFilePath);
                // Return a default duration if we can't determine it
                return 300; // 5 minutes default
            }
        }
    }
} 