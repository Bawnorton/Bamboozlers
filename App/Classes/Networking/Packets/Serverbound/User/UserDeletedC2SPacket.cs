using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.User;

public class UserDeletedC2SPacket : IPacket
{
    public static readonly PacketType<UserDeletedC2SPacket> Type =
        PacketType<UserDeletedC2SPacket>.Create("user_deleted_c2s", json => new UserDeletedC2SPacket(json));
    
    internal int UserId { get; init; }
    internal List<int> ToNotify { get; init; }
    
    private UserDeletedC2SPacket(JsonElement json)
    {
        UserId = json.GetProperty("user_id").GetInt32();
        ToNotify = new List<int>();
        foreach (var userId in json.GetProperty("to_notify").EnumerateArray())
        {
            ToNotify.Add(userId.GetInt32());
        }
    }
    
    internal UserDeletedC2SPacket()
    {
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj.Add("user_id", UserId);
        var toNotify = new JsonArray();
        foreach (var userId in ToNotify)
        {
            toNotify.Add(userId);
        }
        obj.Add("to_notify", toNotify);
    }
}