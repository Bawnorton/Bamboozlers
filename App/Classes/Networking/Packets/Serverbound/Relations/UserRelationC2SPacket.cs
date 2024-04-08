using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.Relations;

public class UserRelationC2SPacket : IPacket
{
    public static readonly PacketType<UserRelationC2SPacket> Type = PacketType<UserRelationC2SPacket>
        .Create("user_relation_c2s", json => new UserRelationC2SPacket(json));
    
    internal RelationAction Action { get; init; }
    internal int SenderId { get; init; }
    internal int ReceiverId { get; init; }
    
    private UserRelationC2SPacket(JsonElement json)
    {
        Action = (RelationAction)json.GetProperty("action").GetInt32();
        SenderId = json.GetProperty("sender_id").GetInt32();
        ReceiverId = json.GetProperty("receiver_id").GetInt32();
    }

    internal UserRelationC2SPacket()
    {
    }

    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj["action"] = (int)Action;
        obj["sender_id"] = SenderId;
        obj["receiver_id"] = ReceiverId;
    }

    internal enum RelationAction
    {
        SendRequest,
        AcceptRequest,
        DeclineRequest,
        RevokeRequest,
        Unblock
    }
}