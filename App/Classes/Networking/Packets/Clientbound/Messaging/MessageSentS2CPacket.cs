using System.Text.Json;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

public class MessageSentS2CPacket : AbstractMessageS2CPacket
{
    public static readonly PacketType<MessageSentS2CPacket> Type =
        PacketType<MessageSentS2CPacket>.Create("message_sent_s2c", json => new MessageSentS2CPacket(json));
    
    internal MessageSentS2CPacket()
    {
    }

    private MessageSentS2CPacket(JsonElement json) : base(json)
    {
    }

    public override PacketType PacketType()
    {
        return Type;
    }
    
}