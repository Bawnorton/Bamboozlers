using System.Text.Json;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound;

public class MessageSentC2SPacket : AbstractMessageC2SPacket
{
    public static readonly PacketType<MessageSentC2SPacket> Type =
        PacketType<MessageSentC2SPacket>.Create("message_sent_c2s",
            json => new MessageSentC2SPacket(json));
    
    internal MessageSentC2SPacket()
    {
    }

    private MessageSentC2SPacket(JsonElement json) : base(json)
    {
    }

    public override PacketType PacketType()
    {
        return Type;
    }
}