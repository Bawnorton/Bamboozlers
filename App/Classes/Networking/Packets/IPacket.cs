using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets;

public interface IPacket
{
    public PacketType PacketType(); 
    
    public void Write(JsonObject obj);

    public string Serialize()
    {
        var obj = new JsonObject();
        Write(obj);
        obj["id"] = PacketType().GetId();
        return obj.ToString();
    }
}