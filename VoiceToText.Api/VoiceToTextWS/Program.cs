using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using VoiceToTextWS;
using VoiceToTextWS.Endpoints;
using VoiceToTextWS.Models;
using VoiceToTextWS.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

#region "OpenAI"
builder.Services.Configure<OpenAIOptions>(
    builder.Configuration.GetSection("OpenAI")
);

builder.Services.AddHttpClient<TranscriptionService>((sp, client) =>
{
    var options = sp
        .GetRequiredService<IOptions<OpenAIOptions>>()
        .Value;
    Console.WriteLine("Configuring HttpClient with BaseUrl: " + options.BaseUrl);
    Console.WriteLine("Using API Key: " + (string.IsNullOrEmpty(options.ApiKey) ? "Not Set" : "Set"));
    client.BaseAddress = new Uri(options.BaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", options.ApiKey);

    client.Timeout = TimeSpan.FromMinutes(2);
});
#endregion

builder.Services.AddSingleton<WebSocketHandler>();

var app = builder.Build();


// Enable WebSockets
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.MapWebSocketEndpoint();

//TestGrpc.TestGrpcAsync().GetAwaiter().GetResult();

app.Run();