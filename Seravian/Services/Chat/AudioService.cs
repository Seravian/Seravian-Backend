using System.Diagnostics;

public interface IAudioService
{
    Task<string> ValidateAndConvertToWavAsync(string inputPath, string outputFolder);
}

public class AudioService : IAudioService
{
    private readonly string _ffmpegPath;

    public AudioService(IWebHostEnvironment env)
    {
        _ffmpegPath = Path.Combine(env.ContentRootPath, "ffmpeg", "ffmpeg.exe");
    }

    public async Task<string> ValidateAndConvertToWavAsync(string inputPath, string outputFolder)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input file not found.");

        // Step 1: Validate audio using FFmpeg - decode check
        var validateArgs = $"-v error -i \"{inputPath}\" -f null -";
        var validateResult = await RunFFmpegAsync(validateArgs);

        if (validateResult.ExitCode != 0)
            throw new InvalidOperationException("Invalid or unsupported audio file.");

        var extension = Path.GetExtension(inputPath).ToLowerInvariant();
        var outputWavPath = Path.Combine(outputFolder, $"{Guid.NewGuid()}.wav");

        // Step 2: If already WAV, optionally re-encode to standard spec, or just return
        if (extension == ".wav")
        {
            // Optional: Re-encode to target format (16kHz mono) for consistency
            // Comment out below if you want to skip re-encoding WAV files

            var reencodeArgs = $"-y -i \"{inputPath}\" -ar 16000 -ac 1 -f wav \"{outputWavPath}\"";
            var reencodeResult = await RunFFmpegAsync(reencodeArgs);

            if (reencodeResult.ExitCode != 0)
                throw new Exception($"Re-encoding WAV failed: {reencodeResult.Error}");

            // Delete original wav file after re-encoding
            File.Delete(inputPath);

            return outputWavPath;
        }

        // Step 3: Convert non-WAV files to WAV
        var convertArgs = $"-y -i \"{inputPath}\" -ar 16000 -ac 1 -f wav \"{outputWavPath}\"";
        var convertResult = await RunFFmpegAsync(convertArgs);

        if (convertResult.ExitCode != 0)
            throw new Exception($"Audio conversion failed: {convertResult.Error}");

        // Delete original non-wav file after conversion
        File.Delete(inputPath);

        return outputWavPath;
    }

    private async Task<(int ExitCode, string Output, string Error)> RunFFmpegAsync(string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, output, error);
    }
}
