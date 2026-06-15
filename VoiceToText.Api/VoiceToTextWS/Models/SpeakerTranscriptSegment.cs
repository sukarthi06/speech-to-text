namespace VoiceToTextWS.Models;

public sealed class SpeakerTranscriptSegment
{
    public string Speaker { get; set; } = "";

    public double Start { get; set; }

    public double End { get; set; }

    public string Text { get; set; } = "";
}
