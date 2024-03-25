using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Networking;
using Bamboozlers.Classes.Networking.Packets.Serverbound;
using Websocket.Client;

namespace Bamboozlers.Classes.Services;

public class WebSocketService : IWebSocketService, IDisposable
{
    private readonly WebsocketClient _client;
    private readonly NetworkHandler _networkHandler = new();

    public WebSocketService(int id)
    {
        _client = new WebsocketClient(new Uri($"ws://localhost:8080/ws/{id}"));
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
        _client.DisconnectionHappened.Subscribe(info => Console.WriteLine($"Disconnected: {info.Type}"));
    }

    public async Task ConnectAsync()
    {
        Console.WriteLine("Connecting to websocket with url: " + _client.Url);
        await _client.Start();
    }

    public void SendPacket(IServerboundPacket packet)
    {
        var obj = new JsonObject();
        packet.Write(obj);
        
        obj.Add("id", packet.PacketType().GetId());
        _client.Send(obj.ToString());
    }
    
    private void PacketRecieved(string packetJson) 
    {
        var json = JsonDocument.Parse(packetJson).RootElement;
        var id = json.GetProperty("id").GetString();
        if (id == null)
        {
            throw new Exception("Received packet with no id. JSON: " + json);
        }
        _networkHandler.HandlePacket(id, json);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}

public interface IWebSocketService
{
    public Task ConnectAsync();
    public void SendPacket(IServerboundPacket packet);
}