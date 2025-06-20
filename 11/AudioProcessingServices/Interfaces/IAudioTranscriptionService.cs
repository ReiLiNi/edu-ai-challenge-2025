using AudioProcessingServices.Models;

namespace AudioProcessingServices.Interfaces
{
    public interface IAudioTranscriptionService
    {
        Task<string> TranscribeAudioAsync(string audioFilePath);
        Task<double> GetAudioDurationAsync(string audioFilePath);
    }
} 