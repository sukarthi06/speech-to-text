using Microsoft.Extensions.AI;
using PhysicianNotes.Application.UseCases.GeneratePhysicianNote;
using PhysicianNotes.Domain.Clinical;
using PhysicianNotes.Domain.Notes;
using System.Text.Json;

namespace PhysicianNotes.Infrastructure.Ai;

public sealed class OpenAiSoapNoteGenerator(IChatClient chatClient) : ISoapNoteGenerator
{
    private readonly IChatClient _chatClient = chatClient;

    public async Task<SoapNote> GenerateAsync(
        ClinicalFacts clinicalFacts,
        CancellationToken cancellationToken = default)
    {
        var prompt =
            $"""
            Generate a physician SOAP note from the provided clinical facts.

            Rules:
            - Use only the provided information.
            - Do not infer diagnoses, causes, or findings.
            - If information is unavailable, keep the section brief.
            - Do not invent medications, findings, or assessments.

            Clinical Facts:
            {JsonSerializer.Serialize(clinicalFacts)}
            """;

        var response = await _chatClient.GetResponseAsync<SoapNote>(
            prompt,
            cancellationToken: cancellationToken);

        if (response.TryGetResult(out var soapNote))
        {
            return soapNote;
        }

        throw new InvalidOperationException(
            "Failed to generate SOAP note.");
    }
}
