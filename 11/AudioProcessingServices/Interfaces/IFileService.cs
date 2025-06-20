using AudioProcessingServices.Models;

namespace AudioProcessingServices.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveTranscriptionAsync(TranscriptionResult result);
        void EnsureOutputDirectoryExists();
        string GenerateTranscriptionFileName();
        Task CopyTranscriptionsToSolutionRootAsync();
    }
} 