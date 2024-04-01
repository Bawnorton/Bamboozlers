using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound;

public class MessageDeletedS2CPacket : IPacket
{
    public static readonly PacketType<MessageDeletedS2CPacket> Type = PacketType<MessageDeletedS2CPacket>.Create("message_deleted_s2c", json => new MessageDeletedS2CPacket(json));
    
    internal int MessageId;
    
    internal MessageDeletedS2CPacket() { }
    
    private MessageDeletedS2CPacket(JsonElement json)
    {
        MessageId = json.GetProperty("message_id").GetInt32();
    }
    
    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj["message_id"] = MessageId;
    }
}