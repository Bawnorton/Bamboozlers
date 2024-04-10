using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;

public class LeaveChatC2SPacket : IPacket
{
    public static readonly PacketType<LeaveChatC2SPacket> Type =
        PacketType<LeaveChatC2SPacket>.Create("leave_chat_c2s", json => new LeaveChatC2SPacket(json));
    
    internal int SenderId;
    internal int ChatId;
    
    internal LeaveChatC2SPacket()
    {
    }
    
    private LeaveChatC2SPacket(JsonElement json)
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