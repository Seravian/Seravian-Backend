using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class WAVService : IWAVService
{
    public void GenerateAndSaveSineWaveWav(
        string filePath,
        double frequency = 440.0,
        int durationSeconds = 2,
        int sampleRate = 24000
    )
    {
        int numChannels = 1;
        short bitsPerSample = 16;
        int byteRate = sampleRate * numChannels * bitsPerSample / 8;
        short blockAlign = (short)(numChannels * bitsPerSample / 8);
        int numSamples = durationSeconds * sampleRate;
        int subchunk2Size = numSamples * numChannels * bitsPerSample / 8;
        int chunkSize = 36 + subchunk2Size;

        using var fs = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        // RIFF header
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(chunkSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt subchunk
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Subchunk1Size
        writer.Write((short)1); // AudioFormat = PCM
        writer.Write((short)numChannels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);

        // data subchunk
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(subchunk2Size);

        // Write sample data
        double amplitude = 32760.0; // Max amplitude for 16-bit audio
        double theta = 2 * Math.PI * frequency / sampleRate;

        for (int i = 0; i < numSamples; i++)
        {
            short sample = (short)(amplitude * Math.Sin(theta * i));
            writer.Write(sample);
        }
    }
}

public interface IWAVService
{
    void GenerateAndSaveSineWaveWav(
        string filePath,
        double frequency = 440.0,
        int durationSeconds = 2,
        int sampleRate = 24000
    );
}
