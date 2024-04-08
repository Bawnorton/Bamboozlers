using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Utility.Observer;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Interaction;

public class InteractionSyncC2SPacket : IPacket
{
    public static readonly PacketType<InteractionSyncC2SPacket> Type = PacketType<InteractionSyncC2SPacket>
        .Create("interaction_sync_c2s", json => new InteractionSyncC2SPacket(json));
    
    internal InteractionEvent Event { get; init; }
    internal int ReceiverId { get; init; }
    
    private InteractionSyncC2SPacket(JsonElement json)
    {
        Event = (InteractionEvent)json.GetProperty("interaction").GetInt32();
        ReceiverId = json.GetProperty("receiver_id").GetInt32();
    }

    internal InteractionSyncC2SPacket()
    {
    }

    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj["interaction"] = (int)Event;
        obj["receiver_id"] = ReceiverId;
    }
}