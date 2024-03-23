using System.Text.Json.Nodes;

namespace Bamboozlers.Classes.Networking.Packets.Serverbound;

public interface IServerboundPacket : IPacket
{
    public void Write(JsonObject obj);
}