using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;

public abstract class AbstractMessageC2SPacket : IPacket
{
    internal int ChatId;
    internal int MessageId;
    internal int SenderId;
    
    internal AbstractMessageC2SPacket()
    {
    }
    
    protected AbstractMessageC2SPacket(JsonElement json)
    {
        SenderId = json.GetProperty("sender_id").GetInt32();
        ChatId = json.GetProperty("chat_id").GetInt32();
        MessageId = json.GetProperty("message_id").GetInt32();
    }
    
    public abstract PacketType PacketType();

    public virtual void Write(JsonObject obj)
    {
        obj["sender_id"] = SenderId;
        obj["chat_id"] = ChatId;
        obj["message_id"] = MessageId;
    }
}