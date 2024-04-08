using Bamboozlers.Classes.Networking.Packets.Clientbound;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;

namespace Bamboozlers.Classes.Networking;

public class ClientNetworkHandler : AbstractNetworkHandler
{
    internal static readonly ClientNetworkHandler Instance = new();

    private ClientNetworkHandler()
    {
        PacketRegistry.RegisterPacket(MessageSentS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessageEditedS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessageDeletedS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessagePinStatusS2CPacket.Type);
    }
}