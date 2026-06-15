using Diarization;
using Grpc.Net.Client;
using VoiceToTextWS.Models;

namespace VoiceToTextWS.Services;

public static class AudioDiarizationService
{
    public static async Task<SpeakerDiarizationResponse?> DiarizeAudioAsync(MemoryStream wavStream)
    {
        // Implementation for audio diarization

        if (wavStream == null) return null;

        using var channel = GrpcChannel.ForAddress("http://localhost:50051");

        var client = new DiarizationService.DiarizationServiceClient(channel);

        var response = await client.ProcessAudioAsync(
            new DiarizationRequest
            {
                AudioData = Google.Protobuf.ByteString.CopyFrom(wavStream.ToArray())
            });                

        var speakerSegments = response.Segments
            .OrderBy(s => s.Start)
            .Select(s => new SpeakerSegments
            {
                Speaker = s.Speaker,
                Start = s.Start,
                End = s.End
            })
            .ToList();

        var speakersInOrder = response.Segments
                .OrderBy(s => s.Start)
                .Select(s => s.Speaker)
                .Distinct()
                .ToList();

        var speakerDiarizationResponse = new SpeakerDiarizationResponse
        {
            SpeakerSegments = speakerSegments,
            SpeakersInOrder = speakersInOrder
        };

        return speakerDiarizationResponse;
    }
}
