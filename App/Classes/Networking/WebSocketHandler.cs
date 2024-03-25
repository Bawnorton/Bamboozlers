using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Networking.Packets.Serverbound;
using Bamboozlers.Classes.Services;
using Websocket.Client;

namespace Bamboozlers.Classes.Networking;

public static class WebSocketHandler
{
    private static readonly NetworkHandler NetworkHandler = new();
    private static WebsocketClient _client;
    private static readonly Uri Url = new("ws://localhost:5180/ws/");

    public static void Init()
    {
        _client = new WebsocketClient(Url);
        _client.ReconnectTimeout = TimeSpan.FromMinutes(10);
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

    public static async Task ConnectAsync(int id)
    {
        _client.Url = new Uri(Url, id.ToString());
        await _client.Start();
    }

    public static void SendPacket(IServerboundPacket packet)
    {
        var obj = new JsonObject();
        packet.Write(obj);
        
        obj.Add("id", packet.PacketType().GetId());
        _client.Send(obj.ToString());
    }
    
    private static void PacketRecieved(string packetJson) 
    {
        var json = JsonDocument.Parse(packetJson).RootElement;
        var id = json.GetProperty("id").GetString();
        if (id == null)
        {
            throw new Exception("Received packet with no id. JSON: " + json);
        }
        NetworkHandler.HandlePacket(id, json);
    }
}