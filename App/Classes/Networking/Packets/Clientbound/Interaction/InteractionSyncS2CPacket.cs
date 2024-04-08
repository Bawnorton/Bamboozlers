using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Utility.Observer;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Interaction;

public class InteractionSyncS2CPacket : IPacket
{
    public static readonly PacketType<InteractionSyncS2CPacket> Type = PacketType<InteractionSyncS2CPacket>
        .Create("interaction_sync_s2c", json => new InteractionSyncS2CPacket(json));
    
    internal InteractionEvent Event { get; init; }
    
    private InteractionSyncS2CPacket(JsonElement json)
    {
        Event = (InteractionEvent)json.GetProperty("interaction").GetInt32();
    }

    internal InteractionSyncS2CPacket()
    {
    }

    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj["interaction"] = (int)Event;
    }
    
}