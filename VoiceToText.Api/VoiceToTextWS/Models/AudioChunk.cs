namespace VoiceToTextWS.Models;

public class AudioChunk
{
    public int ChunkNumber { get; set; }
    public byte[]? Chunk { get; set; }
}
