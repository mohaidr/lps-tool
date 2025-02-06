using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients
{
    public class WebSocketClientService : IDisposable
    {
        private readonly ClientWebSocket _client;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public WebSocketClientService()
        {
            _client = new ClientWebSocket();
            _client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        }

        public async Task ConnectAsync(Uri serverUri, TimeSpan timeout)
        {
            _cancellationTokenSource.CancelAfter(timeout);
            try
            {
                await _client.ConnectAsync(serverUri, _cancellationTokenSource.Token);
                Console.WriteLine("WebSocket connected!");
                await ReceiveAsync();  // Start receiving asynchronously
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Connection timed out.");
            }
        }

        private async Task ReceiveAsync()
        {
            var buffer = new byte[4096];
            try
            {
                while (_client.State == WebSocketState.Open)
                {
                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        Console.WriteLine("WebSocket closed by server.");
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received: {message}");
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            finally
            {
                Dispose();
            }
        }

        public async Task SendAsync(string message)
        {
            if (_client.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client initiated close", CancellationToken.None);
                Console.WriteLine("WebSocket closed.");
            }
            Dispose();
        }

        public void Dispose()
        {
            _client?.Dispose();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
