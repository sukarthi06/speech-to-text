using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using VoiceToTextWS.Models;

namespace VoiceToTextWS.Services;
public class WebSocketHandler(TranscriptionService transcriptionService)
{
    public async Task HandleAsync(WebSocket socket)
    {
        var buffer = new byte[8192];
        Queue<byte[]> bufferQueue = new();

        while (socket.State == WebSocketState.Open)
        {
            using var ms = new MemoryStream();

            WebSocketReceiveResult result;

            do
            {
                result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                }

                ms.Write(buffer, 0, result.Count);

            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Binary)
            {
                byte[] audioBytes = ms.ToArray();

                //Console.WriteLine($"Received {audioBytes.Length} bytes");

                bufferQueue.Enqueue(audioBytes);
                
                //if (bufferQueue.Count == 48)
                //{
                //    var combinedAudio = bufferQueue.SelectMany(b => b).ToArray();
                //    //Console.WriteLine($"Combined audio length: {combinedAudio.Length} bytes");
                //    bufferQueue.Clear();

                //    using var wavStream = await WavStream.CreateWavStreamAsync(combinedAudio);
                //    // TODO: send to Whisper / pipeline
                //    wavStream.Position = 0;
                //    var transcriptionText = await transcriptionService.TranscribeAsync(wavStream);
                //    //Console.WriteLine("Transcription: " + transcriptionText);

                //    await socket.SendAsync(
                //        new ArraySegment<byte>(Encoding.UTF8.GetBytes(transcriptionText)),
                //        WebSocketMessageType.Text,
                //        true,
                //        CancellationToken.None);
                //}
                
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer[..result.Count]);

                if (message == "{\"type\":\"stop\"}")
                {
                    Console.WriteLine("Received stop signal, processing audio...");

                    var combinedAudio = bufferQueue.SelectMany(b => b).ToArray();
                    Console.WriteLine($"Combined audio length: {combinedAudio.Length} bytes");
                    bufferQueue.Clear();

                    using var wavStream = await WavStream.CreateWavStreamAsync(combinedAudio);
                    wavStream.Position = 0;
                    //await TestGrpc.TestGrpcAsync(wavStream); // For testing gRPC with the same audio data
                    var speakerDiarizationResponse = await AudioDiarizationService.DiarizeAudioAsync(wavStream);

                    foreach (var segment in speakerDiarizationResponse?.SpeakerSegments ?? [])
                    {
                        Console.WriteLine($"Speaker: {segment.Speaker}, Start: {segment.Start}, End: {segment.End}");
                    }

                    foreach(var speaker in speakerDiarizationResponse?.SpeakersInOrder ?? [])
                    {
                        Console.WriteLine($"Speaker: {speaker}");
                    }

                    // TODO: send to Whisper / pipeline
                    wavStream.Position = 0;
                    var transcriptionText = await transcriptionService.TranscribeAsync(wavStream);

                    foreach (var segment in transcriptionText.Segments.OrderBy(s => s.Start))
                    {
                        Console.WriteLine($"Segment start: {segment.Start}, End: {segment.End}, Text: {segment.Text}");
                    }

                    //Console.WriteLine("Transcription: " + transcriptionText.Text);

                    //await TestGrpc.TestMergeAsync(speakerDiarizationResponse!, transcriptionText); // For testing merging gRPC responses

                    var mergedResult = await DiarizedTranscription.TestMergeAsync(speakerDiarizationResponse!, transcriptionText);

                    foreach (var segment in mergedResult.OrderBy(s => s.Start))
                    {                        
                        Console.WriteLine($"Speaker: {segment.Speaker}, Text: {segment.Text}");
                    }

                    var response = new TranscriptSpeakerResponse
                    {
                        Text = transcriptionText.Text,
                        Segments = mergedResult
                    };

                    await SendLargeTextAsync(socket, JsonSerializer.Serialize(response));
                    
                    //Console.WriteLine($"Socket state before close: {socket.State}");
                    await socket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Done",
                        CancellationToken.None);

                    break;
                }
            }

            // Client initiated close
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Client requested close");                

                if (socket.State == WebSocketState.CloseReceived || socket.State == WebSocketState.Open)
                {
                    await socket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Goodbye",
                        CancellationToken.None);
                }

                break;
            }
        }
        
    }

    private async Task SendLargeTextAsync(WebSocket socket, string text)
    {
        const int chunkSize = 4096;

        var bytes = Encoding.UTF8.GetBytes(text);
        int offset = 0;
        //Console.WriteLine("Sending transcription in chunks... Total bytes: " + bytes.Length);
        //Console.WriteLine($"Socket state before send: {socket.State}");
        while (offset < bytes.Length)
        {
            var count = Math.Min(chunkSize, bytes.Length - offset);
            //Console.WriteLine($"Sending chunk {offset}-{offset + count}");
            await socket.SendAsync(
                new ArraySegment<byte>(bytes, offset, count),
                WebSocketMessageType.Text,
                offset + count >= bytes.Length,
                CancellationToken.None);

            offset += count;
        }
    }
}
