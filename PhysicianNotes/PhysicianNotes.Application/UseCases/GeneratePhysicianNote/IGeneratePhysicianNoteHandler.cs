using PhysicianNotes.Domain.Notes;

namespace PhysicianNotes.Application.UseCases.GeneratePhysicianNote;

public interface IGeneratePhysicianNoteHandler
{
    Task<PhysicianNote> HandleAsync(
        GeneratePhysicianNoteRequest request,
        CancellationToken cancellationToken = default);
}
