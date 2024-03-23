using Bamboozlers.Classes.Networking;
using Websocket.Client;

namespace Bamboozlers.Classes.Services;

public class WebSocketService : IWebSocketService, IDisposable
{
    private readonly WebsocketClient _client;
    private readonly Uri _serverUri = new("ws://localhost:8000/ws");

    public WebSocketService()
    {
        _client = new WebsocketClient(_serverUri);
        _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _client.ReconnectionHappened.Subscribe(info =>
            Console.WriteLine($"Reconnected: {info.Type}"));
        _client.MessageReceived.Subscribe(msg =>
        {
            if (msg.Text != null)
            {
                PacketRecieved(msg.Text);
            }
            else
            {
                throw new Exception("Websocket message has no text. Message: " + msg);
            }
        });
    }

    public async Task ConnectAsync(int id)
    {
        _client.Url = new Uri(_serverUri, id.ToString());
        await _client.Start();
    }

    public void SendPacket(IPacket packet)
    {
        _client.Send(packet.AsJson());
    }
    
    private void PacketRecieved(string packetJson) 
    {
        
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
    public void SendPacket(IPacket packet);
}