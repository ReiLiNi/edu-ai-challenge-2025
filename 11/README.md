# Audio Transcription & Analysis Application

This C# application processes audio files using OpenAI's Whisper API for transcription and GPT models for summarization and analysis.

## Features

- **Audio Transcription**: Uses OpenAI's Whisper API to convert speech to text
- **Text Summarization**: Generates concise summaries using GPT models
- **Analytics Extraction**: Calculates:
  - Total word count
  - Speaking speed (words per minute)
  - Frequently mentioned topics with mention counts
- **File Management**: Saves each transcription to a separate timestamped file
- **Automatic File Copying**: Copies transcription results to solution root for easy access
- **Console Output**: Displays results in formatted JSON as specified

## Project Structure

```
AudioTranscriptionApp.sln
├── Audio/                          # Audio files directory
│   ├── AK012clip.mp3              # Sample audio files
│   └── CAR0004.mp3                # (automatically detected)
├── Transcriptions/                 # Transcription results (auto-copied)
│   ├── transcript_20241220_143022.json
│   └── transcript_20241220_151145.json
├── AudioTranscriptionApp/          # Console application
│   ├── Program.cs                  # Main application entry point
│   ├── appsettings.json           # Configuration file
│   ├── AudioTranscriptionApp.csproj
│   └── bin/Debug/net8.0/
│       ├── Audio/                  # Audio files (copied during build)
│       └── Transcriptions/         # Original transcription output
└── AudioProcessingServices/        # Processing services library
    ├── Interfaces/                 # Service interfaces
    ├── Models/                     # Data models
    ├── Services/                   # Service implementations
    └── AudioProcessingServices.csproj
```

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (or Visual Studio Code with C# extension)
- OpenAI API key

## Setup Instructions

### 1. Clone and Build

```bash
git clone <repository>
cd <repository-directory>
dotnet restore
dotnet build
```

### 2. Configure OpenAI API Key

Edit `AudioTranscriptionApp/appsettings.json` and replace `"your-openai-api-key-here"` with your actual OpenAI API key:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-api-key-here",
    "WhisperModel": "whisper-1",
    "GptModel": "gpt-4.1-mini",
    "MaxTokens": 2000,
    "Temperature": 0.3
  }
}
```

### 3. Run the Application

#### Option 1: Using Visual Studio (Recommended)
1. **Open Solution**: Double-click `AudioTranscriptionApp.sln` or open it in Visual Studio
2. **Set Startup Project**: Right-click `AudioTranscriptionApp` in Solution Explorer → "Set as Startup Project"
3. **Add Your Audio Files** (Optional):
   - Place any audio files you want to process in the `Audio/` folder in the solution root
   - Supported formats: MP3, MP4, WAV, M4A, WEBM, MPEG, MPGA
   - Files are automatically detected and will appear in the selection menu
4. **Run the Application**: Press **F5** (Debug) or **Ctrl+F5** (Run without debugging)
5. **Select Audio File**: Choose from the available files in the interactive menu
6. **Wait for Processing**: The app will transcribe, summarize, and analyze your audio
7. **View Results**: See summary and analytics in the console, plus find detailed files in `Transcriptions/` folder

#### Option 2: Using Command Line
```bash
cd AudioTranscriptionApp
dotnet run
```

#### Option 3: Run with Specific Audio File
```bash
cd AudioTranscriptionApp
dotnet run "path/to/your/audio/file.mp3"
```

## Usage

### **File Selection Options**

1. **Interactive File Selection**: Run the application and choose from available audio files in the `Audio/` folder
2. **Command Line**: Pass the audio file path as a command line argument
3. **Default Selection**: Press Enter to automatically select the first available audio file

### **Interactive File Selection Menu**

When you run the application without command line arguments, you'll see:

```
=== AUDIO FILE SELECTION ===

Available audio files:
1. AK012clip.mp3 (1.0 MB)
2. CAR0004.mp3 (2.3 MB)

Options:
- Enter a number (1-2) to select a file
- Press Enter to use the first file
- Type a custom file path

Your choice: 
```

**Note**: The application automatically detects all supported audio formats (MP3, MP4, WAV, M4A, etc.) in the `Audio/` folder and handles different execution contexts (Visual Studio vs command line).

## Visual Studio Complete Workflow

### **Step-by-Step Guide for Visual Studio Users**

#### **1. Setup Your Project**
1. Open `AudioTranscriptionApp.sln` in Visual Studio 2022
2. Ensure `AudioTranscriptionApp` is set as the startup project (bold in Solution Explorer)
3. Build the solution: **Build** → **Build Solution** (or Ctrl+Shift+B)

#### **2. Add Your Audio Files**
1. **Navigate to Solution Root**: Open File Explorer to your project folder
2. **Find the Audio Folder**: Look for the `Audio/` folder at the solution root level
3. **Add Your Files**: 
   - Copy/paste your audio files into the `Audio/` folder
   - Supported formats: **MP3, MP4, WAV, M4A, WEBM, MPEG, MPGA**
   - Files can be any size supported by OpenAI's Whisper API
4. **Rebuild** (optional): Build → Rebuild Solution to ensure files are copied

#### **3. Configure OpenAI API Key**
1. **Open appsettings.json**: In Solution Explorer, expand `AudioTranscriptionApp` → double-click `appsettings.json`
2. **Add Your API Key**: Replace `"your-openai-api-key-here"` with your actual OpenAI API key
3. **Save the file**: Ctrl+S

#### **4. Run and Process Audio**
1. **Start the Application**: Press **F5** (with debugging) or **Ctrl+F5** (without debugging)
2. **Select Audio File**: You'll see a menu like this:
   ```
   === AUDIO FILE SELECTION ===
   
   Available audio files:
   1. AK012clip.mp3 (1.0 MB)
   2. CAR0004.mp3 (2.3 MB)
   3. your-new-file.mp3 (3.5 MB)
   
   Options: 
   - Enter a number (1-3) to select a file
   - Press Enter to use the first file
   - Type a custom file path
   
   Your choice: 
   ```
3. **Choose Your Option**:
   - Type `1`, `2`, `3`, etc. to select a specific file
   - Press **Enter** to automatically use the first file
   - Type a full file path for files outside the Audio folder

#### **5. View Processing Results**
During processing, you'll see:
- File detection and path confirmation
- Transcription progress (may take 1-5 minutes depending on file size)
- Summary generation 
- Analytics calculation
- File saving confirmation

#### **6. Find Your Transcription Results**

**Location 1: Solution Root (Easy Access)**
- **Path**: `YourProject/Transcriptions/`
- **Files**: `transcript_YYYYMMDD_HHMMSS.json`
- **Best for**: Easy access, sharing, permanent storage

**Location 2: Build Directory (Development)**
- **Path**: `YourProject/AudioTranscriptionApp/bin/Debug/net8.0/Transcriptions/`
- **Files**: Same JSON files
- **Best for**: Debugging, development

**What's in Each Transcription File:**
```json
{
  "AudioFileName": "your-file.mp3",
  "CreatedAt": "2024-12-20T14:30:22.123Z",
  "AudioDurationSeconds": 138.5,
  "Transcription": "Full text of the audio transcription...",
  "Summary": "AI-generated summary of the content...",
  "Analytics": {
    "word_count": 1280,
    "speaking_speed_wpm": 132,
    "frequently_mentioned_topics": [
      { "topic": "Customer Onboarding", "mentions": 6 },
      { "topic": "Q4 Roadmap", "mentions": 4 }
    ]
  }
}
```

## File Path Resolution

The application uses smart path resolution to find audio files in different execution contexts:

1. **Visual Studio**: When running from Visual Studio (F5), all audio files are automatically copied to the output directory (`bin/Debug/net8.0/Audio/`)
2. **Command Line**: When running from command line, it searches multiple possible locations:
   - `Audio/` (relative to current directory)
   - `../Audio/` (when running from project directory)
   - `../../../../Audio/` (when running from output directory)
   - Absolute path to output directory

**Features:**
- Automatically detects all supported audio formats in the Audio directory
- Removes duplicates and sorts files alphabetically
- Shows file sizes in the selection menu
- Works seamlessly regardless of execution context

This ensures the application works seamlessly regardless of how it's executed and automatically finds all your audio files.

## Quick Reference for Visual Studio Users

### **📁 Where to Put Audio Files**
```
YourProject/
├── Audio/                    ← PUT YOUR AUDIO FILES HERE
│   ├── your-file1.mp3       ← Drag & drop audio files
│   ├── your-file2.wav       ← Any supported format
│   └── your-file3.m4a       ← Auto-detected by app
```

### **📄 Where to Find Results**
```
YourProject/
├── Transcriptions/           ← FIND RESULTS HERE (Easy Access)
│   ├── transcript_20241220_143022.json  ← Complete transcription
│   └── transcript_20241220_151145.json  ← With summary & analytics
```

### **⚙️ Where to Set API Key**
```
YourProject/
├── AudioTranscriptionApp/
│   └── appsettings.json     ← EDIT THIS FILE
```
Replace: `"ApiKey": "your-openai-api-key-here"`
With: `"ApiKey": "sk-your-actual-api-key"`

### **🚀 How to Run**
1. **F5** in Visual Studio (Debug mode)
2. **Ctrl+F5** in Visual Studio (Run without debugging)
3. Choose audio file from menu
4. Wait for processing (1-5 minutes)
5. Check `Transcriptions/` folder for results

## Transcription File Management

The application uses a dual-location approach for transcription files:

### **Build Directory** (`AudioTranscriptionApp/bin/Debug/net8.0/Transcriptions/`)
- Primary location where transcriptions are initially saved
- Used during development and debugging
- Automatically cleaned when rebuilding the project

### **Solution Root** (`./Transcriptions/`)
- Permanent location for easy access
- Files are automatically copied here after each transcription
- Persists across builds and remains accessible from file explorer
- Ideal for sharing results or archiving transcriptions

### **Benefits**
- **Development**: Keep transcriptions during debugging sessions
- **Production**: Easy access to results outside the build environment
- **Backup**: Automatic duplication ensures no transcription loss
- **Sharing**: Simple file path for accessing results

## Output Format

The application outputs results in the following format:

### Summary
A concise summary of the transcript content.

### Analytics (JSON Format)
```json
{
  "word_count": 1280,
  "speaking_speed_wpm": 132,
  "frequently_mentioned_topics": [
    { "topic": "Customer Onboarding", "mentions": 6 },
    { "topic": "Q4 Roadmap", "mentions": 4 },
    { "topic": "AI Integration", "mentions": 3 }
  ]
}
```

### File Output
Each transcription is saved to timestamped JSON files in **two locations**:

1. **Build Directory**: `AudioTranscriptionApp/bin/Debug/net8.0/Transcriptions/`
2. **Solution Root**: `./Transcriptions/` (automatically copied for easy access)

Each transcription file contains complete details including:
- Original audio filename
- Processing timestamp
- Full transcription text
- Summary
- Analytics data

**Note**: The application automatically copies all transcription results from the build directory to the solution root after each run, ensuring easy access regardless of execution method.

## Configuration Options

### OpenAI Settings
- `ApiKey`: Your OpenAI API key
- `WhisperModel`: Whisper model to use (default: "whisper-1")
- `GptModel`: GPT model for analysis (default: "gpt-4")
- `MaxTokens`: Maximum tokens for GPT responses
- `Temperature`: Creativity level for GPT responses

### Transcription Settings
- `OutputDirectory`: Where to save transcription files
- `FilePrefix`: Prefix for transcription filenames
- `DateFormat`: Timestamp format for filenames

### Analytics Settings
- `TopicExtractionPrompt`: Custom prompt for topic extraction
- `SummaryPrompt`: Custom prompt for summarization

## Supported Audio Formats

The application automatically detects and supports various audio formats that OpenAI's Whisper API accepts:
- **MP3** - Most common audio format
- **MP4** - Video files with audio tracks
- **MPEG** - MPEG audio files
- **MPGA** - MPEG audio
- **M4A** - Apple audio format
- **WAV** - Uncompressed audio
- **WEBM** - Web media format

**Note**: Just place any supported audio file in the `Audio/` folder and it will automatically appear in the selection menu.

## Error Handling

The application includes comprehensive error handling for:
- Missing audio files
- API authentication issues
- Network connectivity problems
- Invalid audio formats
- Configuration errors

## Dependencies

### AudioTranscriptionApp
- Microsoft.Extensions.Configuration (8.0.0)
- Microsoft.Extensions.DependencyInjection (8.0.0)
- Microsoft.Extensions.Hosting (8.0.0)
- Microsoft.Extensions.Logging (8.0.0)
- Newtonsoft.Json (13.0.3)

### AudioProcessingServices
- OpenAI (2.0.0)
- Microsoft.Extensions.Configuration.Binder (8.0.0)
- Microsoft.Extensions.Logging.Abstractions (8.0.0)
- Newtonsoft.Json (13.0.3)

## Troubleshooting

### Common Issues

#### **Visual Studio Specific Issues**

1. **"No audio files found in the Audio directory"**
   - **Solution**: Place audio files in the `Audio/` folder at the solution root (same level as `.sln` file)
   - **Check**: Build the solution to ensure files are copied to output directory
   - **Verify**: Look for `Audio/` folder in Solution Explorer

2. **"StartUp project not set" error**
   - **Solution**: Right-click `AudioTranscriptionApp` in Solution Explorer → "Set as Startup Project"
   - **Verify**: Project name should appear **bold** in Solution Explorer

3. **Console window closes immediately**
   - **Solution**: Use **Ctrl+F5** (Run without debugging) instead of F5
   - **Alternative**: Add `Console.ReadKey()` at the end if needed

4. **"Transcription files not found"**
   - **Check Location 1**: `YourProject/Transcriptions/` (solution root)
   - **Check Location 2**: `YourProject/AudioTranscriptionApp/bin/Debug/net8.0/Transcriptions/`
   - **Solution**: Files are auto-copied after processing completes

#### **General Issues**

5. **"OpenAI API key not found"**
   - Ensure your API key is properly set in `AudioTranscriptionApp/appsettings.json`
   - Verify the API key is valid and has sufficient credits
   - Format: `"ApiKey": "sk-your-actual-key-here"`

6. **"Audio file not found"**
   - Check the file path is correct
   - Ensure the file exists and is accessible
   - Verify supported format (MP3, MP4, WAV, M4A, WEBM, MPEG, MPGA)

7. **Build errors**
   - Run `dotnet restore` to restore NuGet packages
   - Ensure you have .NET 8.0 SDK installed
   - Clean and rebuild solution in Visual Studio

### Getting OpenAI API Key
1. Visit [OpenAI's website](https://openai.com)
2. Sign up for an account
3. Navigate to API keys section
4. Generate a new API key
5. Copy the key to your `appsettings.json` file

## Performance Notes

- Processing time depends on audio length and OpenAI API response times
- Larger audio files will take longer to process
- Network speed affects transcription and analysis times
- The application processes transcription, summarization, and topic extraction in parallel where possible

## License

This project is provided as-is for educational and development purposes.

## Version Control (.gitignore)

The project includes a comprehensive `.gitignore` file configured for Visual Studio and .NET development:

### **Excluded from Version Control:**
- **Build artifacts**: `bin/`, `obj/`, `.vs/` directories
- **Generated files**: Transcription results, temporary files, logs
- **User-specific files**: `*.user`, `*.suo`, user settings
- **Security sensitive**: API keys in local config files
- **Large files**: Audio files are tracked by default, but can be excluded if needed

### **Included in Version Control:**
- **Source code**: All `.cs`, `.csproj`, `.sln` files
- **Configuration templates**: `appsettings.json` (with placeholder API key)
- **Documentation**: `README.md`, project documentation
- **Sample audio files**: Files in `Audio/` folder (for testing)

### **Audio Files Policy:**
- **Default**: Audio files in `Audio/` folder ARE tracked (for sample files)
- **Optional**: Uncomment lines in `.gitignore` to exclude audio files if they're too large
- **Recommendation**: Keep small sample files tracked, exclude large production files

### **Transcription Results:**
- **Always excluded**: `Transcriptions/` folder is never committed
- **Reason**: These are generated outputs, not source code
- **Local only**: Results remain on your local machine for privacy

### **API Keys Security:**
- **Protected**: Local configuration files with real API keys are excluded
- **Safe**: Only template `appsettings.json` with placeholder is tracked
- **Best practice**: Never commit real API keys to version control 