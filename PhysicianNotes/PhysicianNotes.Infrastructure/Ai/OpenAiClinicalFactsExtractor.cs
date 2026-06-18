using Microsoft.Extensions.AI;
using PhysicianNotes.Application.UseCases.GeneratePhysicianNote;
using PhysicianNotes.Domain.Clinical;
using PhysicianNotes.Domain.Transcripts;

namespace PhysicianNotes.Infrastructure.Ai;

public sealed class OpenAiClinicalFactsExtractor(IChatClient chatClient) : IClinicalFactsExtractor
{
    public async Task<ClinicalFacts> ExtractAsync(
        Transcript transcript,
        CancellationToken cancellationToken = default)
    {
        var prompt =
            $"""
            Extract clinically relevant facts from the following medical conversation.

            Rules:
            - Use only information explicitly stated.
            - Do not infer missing information.
            - Return empty collections when information is not mentioned.

            ChiefComplaint:
            A single short phrase describing the primary reason for the visit.

            Symptoms:
            Individual symptoms only.

            Transcript:
            {transcript.ConversationText}
            """;

        var response = await chatClient.GetResponseAsync<ClinicalFacts>(
            prompt,
            cancellationToken: cancellationToken);

        if (response.TryGetResult(out var clinicalFacts))
        {
            return clinicalFacts;
        }

        throw new InvalidOperationException(
            "Failed to extract clinical facts.");
    }
}
