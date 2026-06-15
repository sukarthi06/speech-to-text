using System.Net.Http.Headers;
using System.Text.Json;
using VoiceToTextWS.Models;

namespace VoiceToTextWS.Services;

public class TranscriptionService(HttpClient httpClient)
{
    public async Task<TranscriptionResponse> TranscribeAsync(MemoryStream wavStream)
    {
        using var content = new MultipartFormDataContent();

        var audioContent = new StreamContent(wavStream);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        
        content.Add(audioContent, "file", "audio.wav");
        content.Add(new StringContent("whisper-1"), "model");
        content.Add(new StringContent("en"), "language");

        content.Add(new StringContent("verbose_json"), "response_format");
        content.Add(
            new StringContent("segment"),
            "timestamp_granularities[]"
        );

        var response = await httpClient.PostAsync(
            "https://api.openai.com/v1/audio/transcriptions",
            content
        );

        var responseString = await response.Content.ReadAsStringAsync();
        //Console.WriteLine(responseString);
        //Console.WriteLine("Status: " + response.StatusCode);
        //response.EnsureSuccessStatusCode();

        return ExtractText(responseString);
    }

    private TranscriptionResponse ExtractText(string json)
    {
        var result = JsonSerializer.Deserialize<TranscriptionResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        //Console.WriteLine($"Segments: {result?.Segments.Count}");
        //return result?.Text ?? string.Empty;

        return result ?? new TranscriptionResponse();
    }
    //
    //private string ExtractText(string json) { try { using var doc = JsonDocument.Parse(json); return doc.RootElement.GetProperty("text").GetString()!; } catch { return ""; } }
}
