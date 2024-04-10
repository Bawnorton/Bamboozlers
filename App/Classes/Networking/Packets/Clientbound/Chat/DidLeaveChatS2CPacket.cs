using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Chat;

public class DidLeaveChatS2CPacket : IPacket
{
    public static readonly PacketType<DidLeaveChatS2CPacket> Type =
        PacketType<DidLeaveChatS2CPacket>.Create("did_leave_chat_s2c", json => new DidLeaveChatS2CPacket(json));
    
    internal int ChatId { get; init; } = -1;
    
    internal DidLeaveChatS2CPacket()
    {
    }
    
    private DidLeaveChatS2CPacket(JsonElement json)
    {
        ChatId = json.GetProperty("chat_id").GetInt32();
    }
    
    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj.Add("chat_id", ChatId);
    }
    
}