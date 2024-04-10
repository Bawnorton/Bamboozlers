using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Chat;

public class DidJoinChatS2CPacket : IPacket
{
    public static readonly PacketType<DidJoinChatS2CPacket> Type =
        PacketType<DidJoinChatS2CPacket>.Create("did_join_chat_s2c", json => new DidJoinChatS2CPacket(json));
    
    internal int ChatId { get; init; } = -1;
    
    internal DidJoinChatS2CPacket()
    {
    }
    
    private DidJoinChatS2CPacket(JsonElement json)
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