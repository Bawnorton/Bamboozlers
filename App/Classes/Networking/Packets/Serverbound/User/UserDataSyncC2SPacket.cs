using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound.User;

public class UserDataSyncC2SPacket : IPacket
{
    public static readonly PacketType<UserDataSyncC2SPacket> Type =
        PacketType<UserDataSyncC2SPacket>.Create("user_data_sync_c2s", json => new UserDataSyncC2SPacket(json));
    
    internal int UserId { get; init; }
    internal string Username { get; init; }
    
    private UserDataSyncC2SPacket(JsonElement json)
    {
        UserId = json.GetProperty("user_id").GetInt32();
        Username = json.GetProperty("username").GetString()!;
    }
    
    internal UserDataSyncC2SPacket()
    {
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj.Add("user_id", UserId);
        obj.Add("username", Username);
    }
}