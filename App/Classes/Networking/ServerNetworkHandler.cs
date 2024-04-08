using Bamboozlers.Classes.Networking.Packets.Serverbound;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;

namespace Bamboozlers.Classes.Networking;

public class ServerNetworkHandler : AbstractNetworkHandler
{
    internal static readonly ServerNetworkHandler Instance = new();

    private ServerNetworkHandler()
    {
        PacketRegistry.RegisterPacket(JoinChatC2SPacket.Type);
        PacketRegistry.RegisterPacket(MessageSentC2SPacket.Type);
        PacketRegistry.RegisterPacket(MessageEditedC2SPacket.Type);
        PacketRegistry.RegisterPacket(MessageDeletedC2SPacket.Type);
        PacketRegistry.RegisterPacket(MessagePinStatusC2SPacket.Type);
        PacketRegistry.RegisterPacket(InteractionSyncC2SPacket.Type);
        PacketRegistry.RegisterPacket(GroupInteractionSyncC2SPacket.Type);
    }
}