using Bamboozlers.Classes.Events;
using Bamboozlers.Classes.Networking.Packets.Serverbound;

namespace Bamboozlers.Classes.Networking;

public class ServerNetworkHandler : AbstractNetworkHandler
{
    internal static readonly ServerNetworkHandler Instance = new();

    private ServerNetworkHandler()
    {
        PacketRegistry.RegisterPacket(TellOthersToReadDatabaseC2SPacket.Type, async packet =>
        {
            await NetworkEvents.TellOthersToReadDatabaseRequest.Invoker().Invoke(packet.SenderId, packet.ChatId, packet.DbEntry);
        });
    }
}