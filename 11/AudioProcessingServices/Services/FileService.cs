using AudioProcessingServices.Interfaces;
using AudioProcessingServices.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AudioProcessingServices.Services
{
    public class FileService : IFileService
    {
        private readonly TranscriptionSettings _settings;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IConfiguration configuration,
            ILogger<FileService> logger)
        {
            _settings = configuration.GetSection("Transcription").Get<TranscriptionSettings>() ?? new TranscriptionSettings();
            _logger = logger;
        }

        public async Task<string> SaveTranscriptionAsync(TranscriptionResult result)
        {
            try
            {
                EnsureOutputDirectoryExists();
                
                var fileName = GenerateTranscriptionFileName();
                var filePath = Path.Combine(_settings.OutputDirectory, fileName);

                var transcriptionData = new
                {
                    AudioFileName = result.AudioFileName,
                    CreatedAt = result.CreatedAt,
                    AudioDurationSeconds = result.AudioDurationSeconds,
                    Transcription = result.Text,
                    Summary = result.Summary,
                    Analytics = new
                    {
                        word_count = result.Analytics.WordCount,
                        speaking_speed_wpm = result.Analytics.SpeakingSpeedWpm,
                        frequently_mentioned_topics = result.Analytics.FrequentlyMentionedTopics.Select(t => new
                        {
                            topic = t.Topic,
                            mentions = t.Mentions
                        }).ToList()
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(transcriptionData, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, jsonContent);

                _logger.LogInformation("Transcription saved to file: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transcription to file");
                throw;
            }
        }

        public void EnsureOutputDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_settings.OutputDirectory))
                {
                    Directory.CreateDirectory(_settings.OutputDirectory);
                    _logger.LogInformation("Created output directory: {Directory}", _settings.OutputDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating output directory: {Directory}", _settings.OutputDirectory);
                throw;
            }
        }

        public string GenerateTranscriptionFileName()
        {
            var timestamp = DateTime.Now.ToString(_settings.DateFormat);
            return $"{_settings.FilePrefix}{timestamp}.json";
        }

        public async Task CopyTranscriptionsToSolutionRootAsync()
        {
            try
            {
                // Find the solution root directory (where .sln file is located)
                var solutionRootDir = FindSolutionRootDirectory();
                if (string.IsNullOrEmpty(solutionRootDir))
                {
                    _logger.LogWarning("Could not find solution root directory. Skipping transcription copy.");
                    return;
                }

                var targetTranscriptionsDir = Path.Combine(solutionRootDir, "Transcriptions");
                var sourceTranscriptionsDir = Path.GetFullPath(_settings.OutputDirectory);

                // Ensure target directory exists
                if (!Directory.Exists(targetTranscriptionsDir))
                {
                    Directory.CreateDirectory(targetTranscriptionsDir);
                    _logger.LogInformation("Created Transcriptions directory in solution root: {Directory}", targetTranscriptionsDir);
                }

                // Copy all transcription files
                if (Directory.Exists(sourceTranscriptionsDir))
                {
                    var sourceFiles = Directory.GetFiles(sourceTranscriptionsDir, "*.json");
                    var copiedCount = 0;

                    foreach (var sourceFile in sourceFiles)
                    {
                        var fileName = Path.GetFileName(sourceFile);
                        var targetFile = Path.Combine(targetTranscriptionsDir, fileName);

                        // Copy file (overwrite if exists to ensure latest version)
                        await CopyFileAsync(sourceFile, targetFile);
                        copiedCount++;
                    }

                    if (copiedCount > 0)
                    {
                        _logger.LogInformation("Copied {Count} transcription files to solution root: {Directory}", 
                            copiedCount, targetTranscriptionsDir);
                    }
                }
                else
                {
                    _logger.LogWarning("Source transcriptions directory not found: {Directory}", sourceTranscriptionsDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying transcriptions to solution root");
                // Don't throw - this is not critical to the main functionality
            }
        }

        private string FindSolutionRootDirectory()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var searchDir = currentDir;

            // Look for .sln file in current directory and parent directories
            for (int i = 0; i < 10; i++) // Limit search to prevent infinite loop
            {
                var slnFiles = Directory.GetFiles(searchDir, "*.sln");
                if (slnFiles.Length > 0)
                {
                    _logger.LogDebug("Found solution root at: {Directory}", searchDir);
                    return searchDir;
                }

                var parentDir = Directory.GetParent(searchDir);
                if (parentDir == null)
                    break;

                searchDir = parentDir.FullName;
            }

            // Alternative approach: look relative to application base directory
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            searchDir = baseDir;

            for (int i = 0; i < 10; i++)
            {
                var slnFiles = Directory.GetFiles(searchDir, "*.sln");
                if (slnFiles.Length > 0)
                {
                    _logger.LogDebug("Found solution root via base directory at: {Directory}", searchDir);
                    return searchDir;
                }

                var parentDir = Directory.GetParent(searchDir);
                if (parentDir == null)
                    break;

                searchDir = parentDir.FullName;
            }

            return string.Empty;
        }

        private async Task CopyFileAsync(string sourceFile, string targetFile)
        {
            try
            {
                using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                using var targetStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
                await sourceStream.CopyToAsync(targetStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying file from {Source} to {Target}", sourceFile, targetFile);
                throw;
            }
        }
    }
} 