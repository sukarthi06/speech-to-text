using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using PhysicianNotes.Application.UseCases.GeneratePhysicianNote;
using PhysicianNotes.Infrastructure.Ai;
using System.ClientModel;

namespace PhysicianNotes.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IChatClient>(_ =>
        {
            var apiKey = configuration["OpenAI:ApiKey"]!;
            var model = configuration["OpenAI:Model"]!;

            var credential = new ApiKeyCredential(apiKey);

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(configuration["OpenAI:Endpoint"]!)
            };

            return new OpenAIClient(credential, options)
                .GetChatClient(model)
                .AsIChatClient();
        });

        services.AddScoped<IClinicalFactsExtractor, OpenAiClinicalFactsExtractor>();
        services.AddScoped<ISoapNoteGenerator, OpenAiSoapNoteGenerator>();

        return services;
    }
}
