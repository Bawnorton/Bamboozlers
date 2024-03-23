using System.Text.Json;
using Bamboozlers.Classes.Events;
using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Clientbound;
using Bamboozlers.Classes.Networking.Packets.Serverbound;

namespace Bamboozlers.Classes.Networking;

public class NetworkHandler
{
    private readonly PacketRegistry _packetRegistry = new();

    public NetworkHandler()
    {
        _packetRegistry.RegisterPacket(TellOthersToReadDatabaseC2SPacket.Type);
        
        _packetRegistry.RegisterPacket(ReadDatabaseS2CPacket.Type, packet =>
        {
            NetworkEvents.ReadDatabaseRequest.Invoker().Invoke(packet.GetDatabaseEntry());
        });
    }

    public void HandlePacket(string id, JsonElement json)
    {
        var packetTypeActual = _packetRegistry.GetActualType(id);
        if (packetTypeActual == null)
        {
            throw new Exception($"Packet type {id} not registered");
        }
        if(_packetRegistry.IsServerBound(id))
        {
            throw new Exception($"Received server-bound packet {id} on client");
        }
        if (!_packetRegistry.IsClientBound(id))
        {
            throw new Exception("Received packet with unknown type");
        }
        var packetType = _packetRegistry.GetPacketType(id);
        var packet = packetType.Read(json);
        if (packet.GetType() != packetTypeActual)
        {
            throw new Exception($"Packet type mismatch. Expected {packetTypeActual.Name}, got {packet.GetType().Name}");
        }
        _packetRegistry.HandlePacket(id, (IClientboundPacket)packet);
    }
}