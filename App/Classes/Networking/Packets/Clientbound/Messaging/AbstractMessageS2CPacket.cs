using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

public abstract class AbstractMessageS2CPacket : IPacket
{
    internal int ChatId;
    internal int MessageId;
    
    internal AbstractMessageS2CPacket()
    {
    }
    
    protected AbstractMessageS2CPacket(JsonElement json)
    {
        ChatId = json.GetProperty("chat_id").GetInt32();
        MessageId = json.GetProperty("message_id").GetInt32();
    }
    
    public abstract PacketType PacketType();

    public virtual void Write(JsonObject obj)
    {
        obj["chat_id"] = ChatId;
        obj["message_id"] = MessageId;
    }
}