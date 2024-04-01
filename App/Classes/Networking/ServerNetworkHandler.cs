using Bamboozlers.Classes.Networking.Packets.Serverbound;

namespace Bamboozlers.Classes.Networking;

public class ServerNetworkHandler : AbstractNetworkHandler
{
    internal static readonly ServerNetworkHandler Instance = new();
    
    private ServerNetworkHandler()
    {
        PacketRegistry.RegisterPacket(TellOthersToReadDatabaseC2SPacket.Type);
        PacketRegistry.RegisterPacket(MessageUpdatedC2SPacket.Type);
    }
}