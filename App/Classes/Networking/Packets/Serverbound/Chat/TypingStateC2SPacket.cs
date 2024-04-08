using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;

public class TypingStateC2SPacket : IPacket
{
    public static readonly PacketType<TypingStateC2SPacket> Type =
        PacketType<TypingStateC2SPacket>.Create("typing_state_c2s", json => new TypingStateC2SPacket(json));
    
    internal int SenderId { get; init; }
    internal int ChatId { get; init; }
    internal bool Typing { get; init; }

    internal TypingStateC2SPacket()
    {
    }

    private TypingStateC2SPacket(JsonElement json)
    {
        SenderId = json.GetProperty("sender_id").GetInt32();
        ChatId = json.GetProperty("chat_id").GetInt32();
        Typing = json.GetProperty("typing").GetBoolean();
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj.Add("sender_id", SenderId);
        obj.Add("chat_id", ChatId);
        obj.Add("typing", Typing);
    }
}