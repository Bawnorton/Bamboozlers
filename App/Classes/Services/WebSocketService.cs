using System.Text.Json;
using Bamboozlers.Classes.AppDbContext;
using Websocket.Client;

namespace Bamboozlers.Classes.Services;

using System;
using System.Threading.Tasks;

public class WebSocketService : IWebSocketService, IDisposable
{
    private readonly WebsocketClient _client;
    private readonly Uri _serverUri = new("ws://localhost:8000/ws"); // Adjust the URI to your Python server's address

    public WebSocketService()
    {
        _client = new WebsocketClient(_serverUri);
        _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _client.ReconnectionHappened.Subscribe(info =>
            Console.WriteLine($"Reconnected: {info.Type}"));
        _client.MessageReceived.Subscribe(msg =>
            Console.WriteLine($"Message received: {msg}"));
    }

    public async Task ConnectAsync(int id)
    {
        _client.Url = new Uri(_serverUri, id.ToString());
        await _client.Start();
    }

    public void SendMessage(Message message)
    {
        var json = JsonSerializer.Serialize(message);
        _client.Send(json);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}

public interface IWebSocketService
{
    public Task ConnectAsync(int id);
    public void SendMessage(Message message);
}