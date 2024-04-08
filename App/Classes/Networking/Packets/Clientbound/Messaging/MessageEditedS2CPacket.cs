using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

public class MessageEditedS2CPacket : IPacket
{
    public static readonly PacketType<MessageEditedS2CPacket> Type =
        PacketType<MessageEditedS2CPacket>.Create("message_edited_s2c", json => new MessageEditedS2CPacket(json));

    internal int MessageId;
    internal string NewContent = default!;

    internal MessageEditedS2CPacket()
    {
    }

    private MessageEditedS2CPacket(JsonElement json)
    {
        MessageId = json.GetProperty("message_id").GetInt32();
        NewContent = json.GetProperty("new_content").GetString()!;
    }

    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj["message_id"] = MessageId;
        obj["new_content"] = NewContent;
    }
}