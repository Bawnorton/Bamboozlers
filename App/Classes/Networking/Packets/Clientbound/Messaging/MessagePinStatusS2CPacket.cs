using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

public class MessagePinStatusS2CPacket : IPacket
{
    public static readonly PacketType<MessagePinStatusS2CPacket> Type =
        PacketType<MessagePinStatusS2CPacket>.Create("message_pin_status_s2c", json => new MessagePinStatusS2CPacket(json));

    internal int MessageId;
    internal bool Pinned;
    
    internal MessagePinStatusS2CPacket()
    {
    }

    private MessagePinStatusS2CPacket(JsonElement json)
    {
        MessageId = json.GetProperty("message_id").GetInt32();
        Pinned = json.GetProperty("pinned").GetBoolean();
    }

    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj["message_id"] = MessageId;
        obj["pinned"] = Pinned;
    }
}