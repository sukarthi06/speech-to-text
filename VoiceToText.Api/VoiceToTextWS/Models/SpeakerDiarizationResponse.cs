namespace VoiceToTextWS.Models;

public class SpeakerDiarizationResponse
{
    public List<SpeakerSegments> SpeakerSegments { get; set; } = [];
    public List<string> SpeakersInOrder { get; set; } = [];
}
