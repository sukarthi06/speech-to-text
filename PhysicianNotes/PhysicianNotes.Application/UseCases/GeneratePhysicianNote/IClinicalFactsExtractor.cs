using PhysicianNotes.Domain.Clinical;
using PhysicianNotes.Domain.Transcripts;

namespace PhysicianNotes.Application.UseCases.GeneratePhysicianNote;

public interface IClinicalFactsExtractor
{
    Task<ClinicalFacts> ExtractAsync(
        Transcript transcript,
        CancellationToken cancellationToken = default);
}
