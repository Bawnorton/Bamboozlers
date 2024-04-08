using System.Text.Json;
using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Networking.Packets;

namespace Bamboozlers.Classes.Networking;

public abstract class AbstractNetworkHandler
{
    protected readonly PacketRegistry PacketRegistry = new();

    public async Task Handle(string packetJson, AsyncConsumer<IPacket> whenRecieved)
    {
        var json = JsonDocument.Parse(packetJson).RootElement;
        var packetId = json.GetProperty("id").GetString();
        if (packetId == null) throw new Exception($"Packet: {packetJson} does not have an id");
        var expectedType = PacketRegistry.GetActualType(packetId);
        var packet = PacketRegistry.GetPacketType(packetId).Read(json);
        if (packet.GetType() != expectedType) throw new Exception($"Packet: {packetJson} is not of type {expectedType}");
        await whenRecieved((IPacket)packet);
    }
}