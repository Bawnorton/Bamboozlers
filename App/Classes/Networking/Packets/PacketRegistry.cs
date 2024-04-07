namespace Bamboozlers.Classes.Networking.Packets;

public class PacketRegistry
{
    private readonly Dictionary<string, PacketInfo> _packets = new();

    public void RegisterPacket<T>(PacketType<T> packetType) where T : IPacket
    {
        var packetId = packetType.GetId();
        if (_packets.ContainsKey(packetId)) throw new Exception($"Packet type {packetType.GetId()} already registered");

        _packets.Add(packetId, new PacketInfo(packetType, typeof(T)));
    }

    public PacketType GetPacketType(string id)
    {
        return _packets[id].PacketType;
    }

    public Type GetActualType(string id)
    {
        return _packets[id].ActualType;
    }

    private record PacketInfo(PacketType PacketType, Type ActualType);
}