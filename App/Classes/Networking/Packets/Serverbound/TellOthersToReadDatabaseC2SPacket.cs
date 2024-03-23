using System.Text.Json.Nodes;
using Bamboozlers.Classes.Data;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound;

public class TellOthersToReadDatabaseC2SPacket : IServerboundPacket
{
    internal static readonly PacketType<TellOthersToReadDatabaseC2SPacket> Type = PacketType<TellOthersToReadDatabaseC2SPacket>.Create("tell_others_to_read_db_c2s");

    private int _senderId;
    private int[] _recipientIds;
    private DbEntry _dbEntry;

    private TellOthersToReadDatabaseC2SPacket() { }

    public static TellOthersToReadDatabaseC2SPacket Create(int senderId, IEnumerable<int> recipientIds, DbEntry dataType)
    {
        var packet = new TellOthersToReadDatabaseC2SPacket
        {
            _senderId = senderId,
            _recipientIds = recipientIds.ToArray(),
            _dbEntry = dataType
        };
        return packet;
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj["sender_id"] = _senderId;
        var recipientIds = new JsonArray();
        foreach (var recipientId in _recipientIds)
        {
            recipientIds.Add(recipientId);
        }
        obj["recipient_ids"] = recipientIds;
        obj["db_entry"] = _dbEntry.GetId();
    }
}