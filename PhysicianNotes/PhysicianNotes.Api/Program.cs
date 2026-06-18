using PhysicianNotes.Application.UseCases.GeneratePhysicianNote;
using PhysicianNotes.Domain.Clinical;
using PhysicianNotes.Domain.Transcripts;
using PhysicianNotes.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost(
    "/clinical-facts",
    async (
        Transcript transcript,
        IClinicalFactsExtractor extractor,
        CancellationToken cancellationToken) =>
    {
        var clinicalFacts =
            await extractor.ExtractAsync(
                transcript,
                cancellationToken);

        return Results.Ok(clinicalFacts);
    });

app.MapPost(
    "/soap-note",
    async (
        ClinicalFacts clinicalFacts,
        ISoapNoteGenerator generator,
        CancellationToken cancellationToken) =>
    {
        var soapNote =
            await generator.GenerateAsync(
                clinicalFacts,
                cancellationToken);

        return Results.Ok(soapNote);
    });

app.MapPost(
    "/physician-note",
    async (
        Transcript transcript,
        IClinicalFactsExtractor extractor,
        ISoapNoteGenerator generator,
        CancellationToken cancellationToken) =>
    {
        var clinicalFacts =
            await extractor.ExtractAsync(
                transcript,
                cancellationToken);

        var soapNote =
            await generator.GenerateAsync(
                clinicalFacts,
                cancellationToken);

        return Results.Ok(soapNote);
    });

app.Run();
