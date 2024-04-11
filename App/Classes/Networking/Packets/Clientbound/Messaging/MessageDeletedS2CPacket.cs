using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

public class MessageDeletedS2CPacket : AbstractMessageS2CPacket
{
    public static readonly PacketType<MessageDeletedS2CPacket> Type =
        PacketType<MessageDeletedS2CPacket>.Create("message_deleted_s2c", json => new MessageDeletedS2CPacket(json));
    
    internal MessageDeletedS2CPacket()
    {
    }

    private MessageDeletedS2CPacket(JsonElement json) : base(json)
    {
    }

    public override PacketType PacketType()
    {
        return Type;
    }
}