using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Data;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound;

public class ReadDatabaseS2CPacket : IPacket
{
    internal static readonly PacketType<ReadDatabaseS2CPacket> Type =
        PacketType<ReadDatabaseS2CPacket>.Create("read_database_s2c", json => new ReadDatabaseS2CPacket(json));

    internal DbEntry DbEntry;
    
    internal ReadDatabaseS2CPacket() { }
    
    private ReadDatabaseS2CPacket(JsonElement json)
    {
        DbEntry = DbEntry.FromId(json.GetProperty("db_entry").GetString()!);
    }

    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj["db_entry"] = DbEntry.GetId();
    }
}