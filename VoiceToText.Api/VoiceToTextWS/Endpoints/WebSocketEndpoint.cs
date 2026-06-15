using VoiceToTextWS.Services;

namespace VoiceToTextWS.Endpoints;
public static class WebSocketEndpoint
{
    public static void MapWebSocketEndpoint(this WebApplication app)
    {
        app.Map("/ws", async context =>
        {
            Console.WriteLine("WS endpoint hit");

            if (context.WebSockets.IsWebSocketRequest)
            {
                Console.WriteLine("WebSocket request");

                using var socket =
                    await context.WebSockets.AcceptWebSocketAsync();

                Console.WriteLine("Client Connected");

                var handler = context.RequestServices
                                     .GetRequiredService<WebSocketHandler>();

                await handler.HandleAsync(socket);
            }
            else
            {
                Console.WriteLine("Not a WebSocket request");
                context.Response.StatusCode = 400;
            }
        });
    }
}
