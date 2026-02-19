using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using WebApi.Core;

namespace WebApi.Services;

public class PitchStreamService
{
    private readonly PitchDetector _detector;

    public PitchStreamService(PitchDetector detector)
    {
        _detector = detector;
    }

    public async Task HandleConnectionAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        await foreach (var frame in ReceiveFrames(socket))
        {
            float pitch = _detector.DetectPitch(frame);

            Debug.WriteLine(pitch.ToString("F1"));

            var responseBytes =
                Encoding.UTF8.GetBytes(pitch.ToString("F1"));

            await socket.SendAsync(
                responseBytes,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }

    private async IAsyncEnumerable<float[]> ReceiveFrames(WebSocket socket)
    {
        var buffer = new byte[8192];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
                yield break;

            int floatCount = result.Count / 4;
            float[] samples = new float[floatCount];

            Buffer.BlockCopy(buffer, 0, samples, 0, result.Count);

            yield return samples;
        }
    }
}
