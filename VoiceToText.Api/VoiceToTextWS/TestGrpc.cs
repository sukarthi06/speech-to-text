using Diarization;
using Grpc.Net.Client;
using VoiceToTextWS.Models;

namespace VoiceToTextWS
{
    public static class TestGrpc
    {
        public static async Task TestGrpcAsync()
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:50051");

            var client = new DiarizationService.DiarizationServiceClient(channel);

            var response = await client.ProcessAudioAsync(
                new DiarizationRequest
                {
                    AudioData = Google.Protobuf.ByteString.CopyFrom(new byte[] { 1, 2, 3 })
                });

            Console.WriteLine($"Segments: {response.Segments.Count}");
        }

        public static async Task TestGrpcAsync(MemoryStream audioData)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:50051");

            var client = new DiarizationService.DiarizationServiceClient(channel);

            var response = await client.ProcessAudioAsync(
                new DiarizationRequest
                {
                    AudioData = Google.Protobuf.ByteString.CopyFrom(audioData.ToArray())
                });

            Console.WriteLine($"Segments: {response.Segments.Count}");
                      

            foreach (var segment in response.Segments.OrderBy(s => s.Start))
            {
                var start = TimeSpan.FromSeconds(segment.Start);
                var end = TimeSpan.FromSeconds(segment.End);

                //Console.WriteLine(
                //    $"{segment.Speaker} | " +
                //    $"{start:mm\\:ss\\.ff} -> {end:mm\\:ss\\.ff}");
                Console.WriteLine(
                    $"{segment.Speaker} | " +
                    $"start: {segment.Start} -> end: {segment.End}");

            }


            var firstSpeaker = response.Segments.OrderBy(s => s.Start).FirstOrDefault()?.Speaker;

            var speakersInOrder = response.Segments
                .OrderBy(s => s.Start)
                .Select(s => s.Speaker)
                .Distinct()
                .ToList();

            int speakerIndex = 0;
            foreach (var speaker in speakersInOrder)
            {
                Console.WriteLine($"Speaker {++speakerIndex}: {speaker}");
            }
        }

        public static async Task TestMergeAsync(SpeakerDiarizationResponse response, TranscriptionResponse transcription)
        {
            var speakerSegments = response.SpeakerSegments.OrderBy(s => s.Start).ToList();
            var speakersInOrder = response.SpeakerSegments
                .OrderBy(s => s.Start)
                .Select(s => s.Speaker)
                .Distinct()
                .ToList();
            var transcriptSegments = transcription.Segments.OrderBy(s => s.Start).ToList();

            

            var results = new List<SpeakerTranscriptSegment>();

            foreach (var transcriptSegment in transcriptSegments)
            {
                var overlapBySpeaker = new Dictionary<string, double>();

                foreach (var speakerSegment in speakerSegments)
                {
                    double overlap =
                        Math.Max(
                            0,
                            Math.Min(transcriptSegment.End, speakerSegment.End)
                            - Math.Max(transcriptSegment.Start, speakerSegment.Start)
                        );
                    if (overlap > 0)
                    {
                        overlapBySpeaker.TryAdd(
                            speakerSegment.Speaker,
                            0);

                        overlapBySpeaker[speakerSegment.Speaker] += overlap;
                    }
                }
                var bestSpeaker = overlapBySpeaker
                    .OrderByDescending(kv => kv.Value)
                    .FirstOrDefault().Key;

                results.Add(new SpeakerTranscriptSegment
                {
                    Speaker = bestSpeaker,
                    Start = transcriptSegment.Start,
                    End = transcriptSegment.End,
                    Text = transcriptSegment.Text
                });
            }

            foreach (var speakerTranscriptSegment in results)
            {
                Console.WriteLine(
                    $"{speakerTranscriptSegment.Speaker} | " +
                    $"start: {speakerTranscriptSegment.Start} -> end: {speakerTranscriptSegment.End} | " +
                    $"text: {speakerTranscriptSegment.Text}");
            }

            var merged = new List<SpeakerTranscriptSegment>();

            foreach (var segment in results)
            {
                if (merged.Count == 0)
                {
                    merged.Add(segment);
                    continue;
                }

                var last = merged[^1];

                if (last.Speaker == segment.Speaker)
                {
                    // merge
                    if (last.Speaker == segment.Speaker)
                    {
                        last.End = segment.End;

                        last.Text += " " + segment.Text.Trim();
                    }
                }
                else
                {
                    // start new group
                    merged.Add(segment);
                }
            }

            foreach (var mergedSpeakerTranscriptSegment in merged) { 

                Console.WriteLine();
            }
        }
    }
}
