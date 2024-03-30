using System.Text.Json;
using System.Text.Json.Nodes;
using Bamboozlers.Classes.Data;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound;

public class TellOthersToReadDatabaseC2SPacket : IPacket
{
    public static readonly PacketType<TellOthersToReadDatabaseC2SPacket> Type = PacketType<TellOthersToReadDatabaseC2SPacket>.Create("tell_others_to_read_db_c2s", json => new TellOthersToReadDatabaseC2SPacket(json));

    internal int SenderId;
    internal string[] RecipientIdentityNames;
    internal DbEntry DbEntry;

    internal TellOthersToReadDatabaseC2SPacket() { }

    private TellOthersToReadDatabaseC2SPacket(JsonElement json)
    {
        SenderId = json.GetProperty("sender_id").GetInt32();
        var recipientIds = json.GetProperty("recipient_ids");
        RecipientIdentityNames = new string[recipientIds.GetArrayLength()];
        for (var i = 0; i < RecipientIdentityNames.Length; i++)
        {
            RecipientIdentityNames[i] = recipientIds[i].GetString()!;
        }
        DbEntry = DbEntry.FromId(json.GetProperty("db_entry").GetString()!);
    }
    
    public PacketType PacketType()
    {
        return Type;
    }

    public void Write(JsonObject obj)
    {
        obj["sender_id"] = SenderId;
        var recipientIds = new JsonArray();
        foreach (var recipientId in RecipientIdentityNames)
        {
            recipientIds.Add(recipientId);
        }
        obj["recipient_ids"] = recipientIds;
        obj["db_entry"] = DbEntry.GetId();
    }
}