using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound;

public class MessageSentS2CPacket : IPacket
{
    public static readonly PacketType<MessageSentS2CPacket> Type =
        PacketType<MessageSentS2CPacket>.Create("message_sent_s2c", json => new MessageSentS2CPacket(json));

    internal int MessageId;
    
    internal MessageSentS2CPacket()
    {
    }

    private MessageSentS2CPacket(JsonElement json)
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