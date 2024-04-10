using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Utility.Observer;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Interaction;

public class GroupInteractionSyncC2SPacket : IPacket
{
    public static readonly PacketType<GroupInteractionSyncC2SPacket> Type = PacketType<GroupInteractionSyncC2SPacket>
        .Create("group_interaction_sync_c2s", json => new GroupInteractionSyncC2SPacket(json));
    
    internal GroupEvent Event { get; init; }
    internal int GroupId { get; init; }
    internal int SpecificUserId { get; init; }
    
    private GroupInteractionSyncC2SPacket(JsonElement json)
    {
        Event = (GroupEvent)json.GetProperty("group_interaction").GetInt32();
        GroupId = json.GetProperty("group_id").GetInt32();
        SpecificUserId = json.GetProperty("specific_user_id").GetInt32();
    }

    internal GroupInteractionSyncC2SPacket()
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
        obj["specific_user_id"] = SpecificUserId;
    }
}