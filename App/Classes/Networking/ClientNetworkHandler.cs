using Bamboozlers.Classes.Networking.Packets.Clientbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;
using Bamboozlers.Classes.Networking.Packets.Clientbound.User;

namespace Bamboozlers.Classes.Networking;

public class ClientNetworkHandler : AbstractNetworkHandler
{
    internal static readonly ClientNetworkHandler Instance = new();

    private ClientNetworkHandler()
    {
        PacketRegistry.RegisterPacket(DidJoinChatS2CPacket.Type);
        PacketRegistry.RegisterPacket(DidLeaveChatS2CPacket.Type);
        PacketRegistry.RegisterPacket(TypingStateS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessageSentS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessageEditedS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessageDeletedS2CPacket.Type);
        PacketRegistry.RegisterPacket(MessagePinStatusS2CPacket.Type);
        PacketRegistry.RegisterPacket(InteractionSyncS2CPacket.Type);
        PacketRegistry.RegisterPacket(GroupInteractionSyncS2CPacket.Type);
        PacketRegistry.RegisterPacket(UserDataSyncS2CPacket.Type);
    }
}