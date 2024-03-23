using System.Text.Json;
using Bamboozlers.Classes.Data;

namespace Bamboozlers.Classes.Networking.Packets.Clientbound;

public class ReadDatabaseS2CPacket : IClientboundPacket
{
    internal static readonly PacketType<ReadDatabaseS2CPacket> Type =
        PacketType<ReadDatabaseS2CPacket>.Create("read_database_s2c", json => new ReadDatabaseS2CPacket(json));

    private readonly DbEntry _dbEntry;

    private ReadDatabaseS2CPacket(JsonElement json)
    {
        _dbEntry = DbEntry.FromId(json.GetProperty("db_entry").GetString()!);
    }

    public PacketType PacketType()
    {
        return Type;
    }

    public DbEntry GetDatabaseEntry()
    {
        return _dbEntry;
    }
}