using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

public class MessagePinStatusS2CPacket : AbstractMessageS2CPacket
{
    public static readonly PacketType<MessagePinStatusS2CPacket> Type =
        PacketType<MessagePinStatusS2CPacket>.Create("message_pin_status_s2c", json => new MessagePinStatusS2CPacket(json));

    internal bool Pinned;
    
    internal MessagePinStatusS2CPacket()
    {
    }

    private MessagePinStatusS2CPacket(JsonElement json) : base(json)
    {
        Pinned = json.GetProperty("pinned").GetBoolean();
    }

    public override PacketType PacketType()
    {
        return Type;
    }
    
    public override void Write(JsonObject obj)
    {
        base.Write(obj);
        obj["pinned"] = Pinned;
    }
}