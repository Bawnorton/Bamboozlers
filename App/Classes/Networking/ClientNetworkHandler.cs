using Bamboozlers.Classes.Networking.Packets.Clientbound;

namespace Bamboozlers.Classes.Networking;

public class ClientNetworkHandler : AbstractNetworkHandler
{
    internal static readonly ClientNetworkHandler Instance = new();

    private ClientNetworkHandler()
    {
        PacketRegistry.RegisterPacket(ReadDatabaseS2CPacket.Type);
    }
}