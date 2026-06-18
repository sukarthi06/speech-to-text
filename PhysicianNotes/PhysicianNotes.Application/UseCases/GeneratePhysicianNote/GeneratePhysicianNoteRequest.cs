using PhysicianNotes.Domain.Transcripts;

namespace PhysicianNotes.Application.UseCases.GeneratePhysicianNote;

public record GeneratePhysicianNoteRequest(
    Transcript Transcript);
