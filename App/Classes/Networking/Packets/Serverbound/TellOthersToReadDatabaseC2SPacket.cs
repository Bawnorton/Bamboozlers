using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Data;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound;

public class TellOthersToReadDatabaseC2SPacket : IPacket
{
    public static readonly PacketType<TellOthersToReadDatabaseC2SPacket> Type =
        PacketType<TellOthersToReadDatabaseC2SPacket>.Create("tell_others_to_read_db_c2s",
            json => new TellOthersToReadDatabaseC2SPacket(json));

    internal int ChatId;
    internal DbEntry DbEntry = default!;
    internal int SenderId;

    internal TellOthersToReadDatabaseC2SPacket()
    {
    }

    private TellOthersToReadDatabaseC2SPacket(JsonElement json)
    {
        SenderId = json.GetProperty("sender_id").GetInt32();
        ChatId = json.GetProperty("chat_id").GetInt32();
        DbEntry = DbEntry.FromId(json.GetProperty("db_entry").GetString()!);
    }

    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj["sender_id"] = SenderId;
        obj["chat_id"] = ChatId;
        obj["db_entry"] = DbEntry.GetId();
    }
}