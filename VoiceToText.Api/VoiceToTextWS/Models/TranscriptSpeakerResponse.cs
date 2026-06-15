namespace VoiceToTextWS.Models;

public class TranscriptSpeakerResponse
{
    public string Text { get; set; } = "";

    public List<SpeakerTranscriptSegment> Segments { get; set; } = [];
}
