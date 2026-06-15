using VoiceToTextWS.Models;

namespace VoiceToTextWS.Services;

public static class DiarizedTranscription
{
    public static async Task<List<SpeakerTranscriptSegment>> TestMergeAsync(SpeakerDiarizationResponse response, TranscriptionResponse transcription)
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
        return merged;
    }
}
