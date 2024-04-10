using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;

public class MessagePinStatusC2SPacket : AbstractMessageC2SPacket
{
    public static readonly PacketType<MessagePinStatusC2SPacket> Type =
        PacketType<MessagePinStatusC2SPacket>.Create("message_pin_status_c2s", json => new MessagePinStatusC2SPacket(json));
    
    internal bool Pinned;
    
    internal MessagePinStatusC2SPacket()
    {
    }
    
    private MessagePinStatusC2SPacket(JsonElement json) : base(json)
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