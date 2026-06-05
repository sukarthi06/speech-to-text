using VoiceToTextWS.Services;

namespace VoiceToTextWS.Endpoints;
public static class WebSocketEndpoint
{
    public static void MapWebSocketEndpoint(this WebApplication app)
    {
        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var socket =
                    await context.WebSockets.AcceptWebSocketAsync();

                Console.WriteLine("Client Connected");

                var handler = context.RequestServices
                                     .GetRequiredService<WebSocketHandler>();

                await handler.HandleAsync(socket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });
    }
}
