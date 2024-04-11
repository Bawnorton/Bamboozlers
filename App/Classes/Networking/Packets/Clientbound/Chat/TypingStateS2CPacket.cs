using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Chat;

public class TypingStateS2CPacket : IPacket
{
    public static readonly PacketType<TypingStateS2CPacket> Type =
        PacketType<TypingStateS2CPacket>.Create("typing_state_s2c", json => new TypingStateS2CPacket(json));
    
    internal int UserId { get; init; }
    internal int ChatId { get; init; }
    internal bool Typing { get; init; }

    internal TypingStateS2CPacket()
    {
    }

    private TypingStateS2CPacket(JsonElement json)
    {
        UserId = json.GetProperty("user_id").GetInt32();
        ChatId = json.GetProperty("chat_id").GetInt32();
        Typing = json.GetProperty("typing").GetBoolean();
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj.Add("user_id", UserId);
        obj.Add("chat_id", ChatId);
        obj.Add("typing", Typing);
    }
}