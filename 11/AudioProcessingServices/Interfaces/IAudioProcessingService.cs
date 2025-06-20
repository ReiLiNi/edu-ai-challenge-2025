using AudioProcessingServices.Models;

namespace AudioProcessingServices.Interfaces
{
    public interface IAudioProcessingService
    {
        Task<TranscriptionResult> ProcessAudioAsync(string audioFilePath);
    }
} 