using Bamboozlers.Classes.Networking.Packets.Serverbound;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Relations;

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
        PacketRegistry.RegisterPacket(UserRelationC2SPacket.Type);
    }
}