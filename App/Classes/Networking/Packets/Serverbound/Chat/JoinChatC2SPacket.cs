using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;

public class JoinChatC2SPacket : IPacket
{
    public static readonly PacketType<JoinChatC2SPacket> Type =
        PacketType<JoinChatC2SPacket>.Create("join_chat_c2s", json => new JoinChatC2SPacket(json));
    
    internal int SenderId;
    internal int ChatId;
    
    internal JoinChatC2SPacket()
    {
    }
    
    private JoinChatC2SPacket(JsonElement json)
    {
        SenderId = json.GetProperty("sender_id").GetInt32();
        ChatId = json.GetProperty("chat_id").GetInt32();
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj["sender_id"] = SenderId;
        obj["chat_id"] = ChatId;
    }
}