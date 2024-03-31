using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Clientbound;
using Bamboozlers.Classes.Networking.Packets.Serverbound;
using Microsoft.AspNetCore.SignalR;

namespace Bamboozlers.Classes.Networking.SignalR;

/*
 * Normally you'd auth something like this, which would also give the convenience of using the User property in the Hub
 * Alas, 4 year old issue that is still open
 * https://github.com/dotnet/aspnetcore/issues/25000
 * The workarounds mentioned in the issue do not work with our current setup (Because of course not), I tried all of them
 */

// ReSharper disable UnusedMember.Global - This class is used by the SignalR framework
public class BamboozlersHub : Hub
{
    public const string HubUrl = "/bamboozlers_chat";
    
    public async Task ReceivePacketOnServer(string packetJson)
    {
        await ServerNetworkHandler.Instance.Handle(packetJson, async packet =>
        {
            switch (packet)
            {
                case TellOthersToReadDatabaseC2SPacket tellOthersToReadDatabaseC2SPacket:
                    var readDatabaseS2CPacket = new ReadDatabaseS2CPacket
                    {
                        DbEntry = tellOthersToReadDatabaseC2SPacket.DbEntry
                    };
                    await Clients.Groups(tellOthersToReadDatabaseC2SPacket.ChatId.ToString()).SendAsync("RecievePacketOnClient", ((IPacket)readDatabaseS2CPacket).Serialize());
                    break;
            }
        });
    }

    public async Task JoinChat(int chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        Console.WriteLine($"User {Context.ConnectionId} joined chat {chatId}");
    }
    
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}