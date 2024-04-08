using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;

public class MessageEditedC2SPacket : AbstractMessageC2SPacket
{
    public static readonly PacketType<MessageEditedC2SPacket> Type =
        PacketType<MessageEditedC2SPacket>.Create("message_edited_c2s", json => new MessageEditedC2SPacket(json));

    internal string NewContent;

    internal MessageEditedC2SPacket()
    {
    }

    private MessageEditedC2SPacket(JsonElement json) : base(json)
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