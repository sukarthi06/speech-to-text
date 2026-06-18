namespace PhysicianNotes.Infrastructure.Options;

public sealed class OpenAiOptions
{
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
}
