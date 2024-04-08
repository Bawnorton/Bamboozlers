using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Utility.Observer;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.Interaction;

public class GroupInteractionSyncS2CPacket : IPacket
{
    public static readonly PacketType<GroupInteractionSyncS2CPacket> Type = PacketType<GroupInteractionSyncS2CPacket>
        .Create("group_interaction_sync_s2c", json => new GroupInteractionSyncS2CPacket(json));
    
    internal GroupEvent Event { get; init; }
    internal int GroupId { get; init; }
    
    private GroupInteractionSyncS2CPacket(JsonElement json)
    {
        Event = (GroupEvent)json.GetProperty("group_interaction").GetInt32();
        GroupId = json.GetProperty("group_id").GetInt32();
    }

    internal GroupInteractionSyncS2CPacket()
    {
    }

    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj["group_interaction"] = (int)Event;
        obj["group_id"] = GroupId;
    }
    
}