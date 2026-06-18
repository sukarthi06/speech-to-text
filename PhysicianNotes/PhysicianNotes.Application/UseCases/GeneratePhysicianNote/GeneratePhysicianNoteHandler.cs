using PhysicianNotes.Domain.Notes;

namespace PhysicianNotes.Application.UseCases.GeneratePhysicianNote;

public sealed class GeneratePhysicianNoteHandler
    : IGeneratePhysicianNoteHandler
{
    private readonly IClinicalFactsExtractor _clinicalFactsExtractor;
    private readonly ISoapNoteGenerator _soapNoteGenerator;

    public GeneratePhysicianNoteHandler(
        IClinicalFactsExtractor clinicalFactsExtractor,
        ISoapNoteGenerator soapNoteGenerator)
    {
        _clinicalFactsExtractor = clinicalFactsExtractor;
        _soapNoteGenerator = soapNoteGenerator;
    }

    public async Task<PhysicianNote> HandleAsync(
        GeneratePhysicianNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var clinicalFacts =
            await _clinicalFactsExtractor.ExtractAsync(
                request.Transcript,
                cancellationToken);

        var soapNote =
            await _soapNoteGenerator.GenerateAsync(
                clinicalFacts,
                cancellationToken);

        return new PhysicianNote
        {
            SoapNote = soapNote
        };
    }
}
