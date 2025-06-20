using AudioProcessingServices.Interfaces;
using AudioProcessingServices.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace AudioTranscriptionApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Audio Transcription & Analysis App ===");
            Console.WriteLine();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Setup dependency injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);
            
            var serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                // Get the main processing service
                var audioProcessingService = serviceProvider.GetRequiredService<IAudioProcessingService>();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                // Get audio file path
                string audioFilePath;
                if (args.Length > 0)
                {
                    audioFilePath = args[0];
                }
                else
                {
                    audioFilePath = SelectAudioFile();
                }

                if (!File.Exists(audioFilePath))
                {
                    Console.WriteLine($"Error: Audio file '{audioFilePath}' not found.");
                    return;
                }

                Console.WriteLine($"Processing audio file: {audioFilePath}");
                Console.WriteLine($"File found at: {Path.GetFullPath(audioFilePath)}");
                Console.WriteLine("This may take a few minutes...");
                Console.WriteLine();

                // Process the audio file
                var result = await audioProcessingService.ProcessAudioAsync(audioFilePath);

                // Copy transcription results to solution root directory
                var fileService = serviceProvider.GetRequiredService<IFileService>();
                await fileService.CopyTranscriptionsToSolutionRootAsync();

                // Display results
                DisplayResults(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            finally
            {
                serviceProvider.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string SelectAudioFile()
        {
            Console.WriteLine("=== AUDIO FILE SELECTION ===");
            Console.WriteLine();

            // Get available audio files
            var audioFiles = GetAvailableAudioFiles();

            if (audioFiles.Count == 0)
            {
                Console.WriteLine("No audio files found in the Audio directory.");
                Console.Write("Please enter the full path to your audio file: ");
                return Console.ReadLine() ?? string.Empty;
            }

            // Display available files
            Console.WriteLine("Available audio files:");
            for (int i = 0; i < audioFiles.Count; i++)
            {
                var fileInfo = new FileInfo(audioFiles[i]);
                var sizeInMB = fileInfo.Length / 1024.0 / 1024.0;
                Console.WriteLine($"{i + 1}. {Path.GetFileName(audioFiles[i])} ({sizeInMB:F1} MB)");
            }

            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine($"- Enter a number (1-{audioFiles.Count}) to select a file");
            Console.WriteLine("- Press Enter to use the first file");
            Console.WriteLine("- Type a custom file path");
            Console.WriteLine();
            Console.Write("Your choice: ");

            var input = Console.ReadLine();

            // Handle user input
            if (string.IsNullOrWhiteSpace(input))
            {
                // Default to first file
                return audioFiles[0];
            }

            if (int.TryParse(input, out int selection) && selection >= 1 && selection <= audioFiles.Count)
            {
                // User selected a number
                return audioFiles[selection - 1];
            }

            // User entered a custom path
            return input;
        }

        private static List<string> GetAvailableAudioFiles()
        {
            var audioFiles = new List<string>();
            var supportedExtensions = new[] { "*.mp3", "*.mp4", "*.mpeg", "*.mpga", "*.m4a", "*.wav", "*.webm" };

            // Possible locations for the Audio directory
            var possibleAudioDirectories = new[]
            {
                "Audio",                                    // When running from output directory
                Path.Combine("..", "Audio"),              // When running from project directory
                Path.Combine("..", "..", "..", "..", "Audio"), // When running from bin/Debug/net8.0
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio") // Absolute path to output directory
            };

            foreach (var audioDir in possibleAudioDirectories)
            {
                if (Directory.Exists(audioDir))
                {
                    foreach (var extension in supportedExtensions)
                    {
                        var files = Directory.GetFiles(audioDir, extension, SearchOption.TopDirectoryOnly);
                        audioFiles.AddRange(files);
                    }

                    if (audioFiles.Count > 0)
                    {
                        // Remove duplicates and sort
                        audioFiles = audioFiles.Distinct().OrderBy(f => Path.GetFileName(f)).ToList();
                        break; // Found files, no need to check other directories
                    }
                }
            }

            return audioFiles;
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add configuration
            services.AddSingleton(configuration);

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Register services
            services.AddTransient<IAudioTranscriptionService, AudioTranscriptionService>();
            services.AddTransient<ITextAnalysisService, TextAnalysisService>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IAudioProcessingService, AudioProcessingService>();
        }

        private static void DisplayResults(AudioProcessingServices.Models.TranscriptionResult result)
        {
            Console.WriteLine("=== PROCESSING COMPLETED ===");
            Console.WriteLine();

            Console.WriteLine("=== SUMMARY ===");
            Console.WriteLine(result.Summary);
            Console.WriteLine();

            Console.WriteLine("=== ANALYTICS ===");
            var analyticsJson = JsonConvert.SerializeObject(new
            {
                word_count = result.Analytics.WordCount,
                speaking_speed_wpm = result.Analytics.SpeakingSpeedWpm,
                frequently_mentioned_topics = result.Analytics.FrequentlyMentionedTopics.Select(t => new
                {
                    topic = t.Topic,
                    mentions = t.Mentions
                }).ToList()
            }, Formatting.Indented);

            Console.WriteLine(analyticsJson);
            Console.WriteLine();

            Console.WriteLine("=== ADDITIONAL INFO ===");
            Console.WriteLine($"Audio File: {result.AudioFileName}");
            Console.WriteLine($"Audio Duration: {result.AudioDurationSeconds:F1} seconds");
            Console.WriteLine($"Processed At: {result.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine();
            Console.WriteLine("=== TRANSCRIPTION FILES SAVED ===");
            Console.WriteLine("Transcription results have been saved to:");
            Console.WriteLine("1. Build directory: './AudioTranscriptionApp/bin/Debug/net8.0/Transcriptions/'");
            Console.WriteLine("2. Solution root: './Transcriptions/' (for easy access)");
            Console.WriteLine("Each transcription includes full text, summary, and analytics data.");
        }
    }
} 