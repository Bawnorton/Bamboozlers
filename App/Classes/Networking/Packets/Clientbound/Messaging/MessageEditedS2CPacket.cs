using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

public class MessageEditedS2CPacket : AbstractMessageS2CPacket
{
    public static readonly PacketType<MessageEditedS2CPacket> Type =
        PacketType<MessageEditedS2CPacket>.Create("message_edited_s2c", json => new MessageEditedS2CPacket(json));

    internal string NewContent = default!;

    internal MessageEditedS2CPacket()
    {
    }

    private MessageEditedS2CPacket(JsonElement json) : base(json)
    {
        NewContent = json.GetProperty("new_content").GetString()!;
    }

    public override PacketType PacketType()
    {
        return Type;
    }

    public override void Write(JsonObject obj)
    {
        base.Write(obj);
        obj["new_content"] = NewContent;
    }
}