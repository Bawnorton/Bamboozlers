using Bamboozlers.Classes.Networking.Packets.Clientbound;

namespace Bamboozlers.Classes.Networking;

public class ClientNetworkHandler : AbstractNetworkHandler
{
    internal static readonly ClientNetworkHandler Instance = new();

    private ClientNetworkHandler()
    {
        PacketRegistry.RegisterPacket(ReadDatabaseS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessageEditedS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessageDeletedS2CPacket.Type);
    }
}