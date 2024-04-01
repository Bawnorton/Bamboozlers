using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound;

public class MessageUpdatedC2SPacket : IPacket
{
    public static readonly PacketType<MessageUpdatedC2SPacket> Type = PacketType<MessageUpdatedC2SPacket>.Create("message_updated_c2s", json => new MessageUpdatedC2SPacket(json));
    
    internal int UpdaterId;
    internal int ChatId;
    internal int MessageId;
    internal bool Deleted;
    internal string? NewContent;
    
    internal MessageUpdatedC2SPacket() { }
    
    private MessageUpdatedC2SPacket(JsonElement json)
    {
        UpdaterId = json.GetProperty("updater_id").GetInt32();
        ChatId = json.GetProperty("chat_id").GetInt32();
        MessageId = json.GetProperty("message_id").GetInt32();
        NewContent = json.GetProperty("new_content").GetString();
        Deleted = json.GetProperty("deleted").GetBoolean();
    }
    
    public PacketType PacketType()
    {
        return Type;
    }
    
    public void Write(JsonObject obj)
    {
        obj["updater_id"] = UpdaterId;
        obj["chat_id"] = ChatId;
        obj["message_id"] = MessageId;
        obj["new_content"] = NewContent;
        obj["deleted"] = Deleted;
    }
}