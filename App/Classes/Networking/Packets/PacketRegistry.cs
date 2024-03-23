using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Networking.Packets.Clientbound;
using Bamboozlers.Classes.Networking.Packets.Serverbound;

namespace Bamboozlers.Classes.Networking.Packets;

public class PacketRegistry
{
    private readonly Dictionary<string, PacketInfo> _packets = new();
    
    public void RegisterPacket<T>(PacketType<T> packetType, Consumer<T>? onReceive = default) where T : IPacket 
    {
        if(_packets.ContainsKey(packetType.GetId()))
        {
            throw new Exception($"Packet type {packetType.GetId()} already registered");
        }
        if(onReceive == null && packetType is PacketType<IClientboundPacket>)
        {
            throw new Exception($"No handler provided for clientbound packet type {packetType.GetId()}");
        }

        if (onReceive != null && packetType is PacketType<IServerboundPacket>)
        {
            throw new Exception($"Handler provided for serverbound packet type {packetType.GetId()}");
        }
        _packets.Add(packetType.GetId(), new PacketInfo(packetType, typeof(T), packet => onReceive?.Invoke((T)packet)));
    }
    
    public PacketType GetPacketType(string id)
    {
        return _packets[id].PacketType;
    }
    
    public Type GetActualType(string id)
    {
        return _packets[id].ActualType;
    }
    
    public bool IsClientBound(string id)
    {
        return _packets[id].ActualType.IsAssignableFrom(typeof(IClientboundPacket));
    }
    
    public bool IsServerBound(string id)
    {
        return _packets[id].ActualType.IsAssignableFrom(typeof(IServerboundPacket));
    }
    
    public void HandlePacket(string id, IPacket packet)
    {
        if (!_packets.TryGetValue(id, out var value))
        {
            throw new Exception($"Packet type {id} not registered");
        }
        value.OnReceive!((packet as IClientboundPacket)!);
    }

    private record PacketInfo(PacketType PacketType, Type ActualType, Consumer<IPacket>? OnReceive);
}