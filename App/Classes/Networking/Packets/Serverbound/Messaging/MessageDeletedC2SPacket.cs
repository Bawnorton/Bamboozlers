using System.Text.Json;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;

public class MessageDeletedC2SPacket : AbstractMessageC2SPacket
{
    public static readonly PacketType<MessageDeletedC2SPacket> Type =
        PacketType<MessageDeletedC2SPacket>.Create("message_deleted_c2s", json => new MessageDeletedC2SPacket(json));

    internal MessageDeletedC2SPacket()
    {
    }

    private MessageDeletedC2SPacket(JsonElement json) : base(json)
    {
    }

    public override PacketType PacketType()
    {
        return Type;
    }
}