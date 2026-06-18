using PhysicianNotes.Domain.Clinical;
using PhysicianNotes.Domain.Notes;

namespace PhysicianNotes.Application.UseCases.GeneratePhysicianNote;

public interface ISoapNoteGenerator
{
    Task<SoapNote> GenerateAsync(
        ClinicalFacts clinicalFacts,
        CancellationToken cancellationToken = default);
}
