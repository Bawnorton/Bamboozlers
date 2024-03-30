using Bamboozlers.Classes.Func;

namespace Bamboozlers.Classes.Networking.Packets;

public class PacketRegistry
{
    private readonly Dictionary<string, PacketInfo> _packets = new();
    
    public void RegisterPacket<T>(PacketType<T> packetType, AsyncConsumer<T> onReceive) where T : IPacket 
    {
        if(_packets.ContainsKey(packetType.GetId()))
        {
            throw new Exception($"Packet type {packetType.GetId()} already registered");
        }
        _packets.Add(packetType.GetId(), new PacketInfo(packetType, typeof(T), packet => onReceive((T)packet)));
    }
    
    public PacketType GetPacketType(string id)
    {
        return _packets[id].PacketType;
    }
    
    public Type GetActualType(string id)
    {
        return _packets[id].ActualType;
    }
    
    public async Task HandlePacket(string id, IPacket packet)
    {
        if (!_packets.TryGetValue(id, out var value))
        {
            throw new Exception($"Packet type {id} not registered");
        }

        await value.OnReceive(packet);
    }

    private record PacketInfo(PacketType PacketType, Type ActualType, AsyncConsumer<IPacket> OnReceive);
}