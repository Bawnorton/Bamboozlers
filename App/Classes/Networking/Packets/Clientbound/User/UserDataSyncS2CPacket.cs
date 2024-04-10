using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound.User;

public class UserDataSyncS2CPacket : IPacket
{
    public static readonly PacketType<UserDataSyncS2CPacket> Type =
        PacketType<UserDataSyncS2CPacket>.Create("user_data_sync_s2c", json => new UserDataSyncS2CPacket(json));
    
    internal int UserId { get; init; }
    
    private UserDataSyncS2CPacket(JsonElement json)
    {
        UserId = json.GetProperty("user_id").GetInt32();
    }
    
    internal UserDataSyncS2CPacket()
    {
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj.Add("user_id", UserId);
    }
}