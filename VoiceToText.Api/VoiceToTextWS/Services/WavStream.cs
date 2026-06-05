namespace VoiceToTextWS.Services;

public static class WavStream
{
    public static async Task<MemoryStream> CreateWavStreamAsync(byte[] float32Bytes, int sampleRate = 48000)
    {
        return await Task.Run(() =>
        {
            int sampleCount = float32Bytes.Length / 4;
            short[] int16Samples = new short[sampleCount];

            // Convert Float32 → Int16
            for (int i = 0; i < sampleCount; i++)
            {
                float sample = BitConverter.ToSingle(float32Bytes, i * 4);
                sample = Math.Clamp(sample, -1f, 1f);
                int16Samples[i] = (short)(sample * short.MaxValue);
            }

            short channels = 1;
            short bitsPerSample = 16;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);

            int dataSize = int16Samples.Length * 2;

            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + dataSize);
                writer.Write("WAVE".ToCharArray());

                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((short)1); // PCM
                writer.Write(channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write(bitsPerSample);

                writer.Write("data".ToCharArray());
                writer.Write(dataSize);

                byte[] pcmBytes = new byte[dataSize];
                Buffer.BlockCopy(int16Samples, 0, pcmBytes, 0, dataSize);
                writer.Write(pcmBytes);
            }

            stream.Position = 0;
            return stream;
        });
    }
}