using Bamboozlers.Classes.Events;
using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Clientbound;
using Microsoft.AspNetCore.SignalR;

namespace Bamboozlers.Classes.Networking.SignalR;

// ReSharper disable UnusedMember.Global - This class is used by the SignalR framework
public class BamboozlersHub : Hub
{
    public const string HubUrl = "/bamboozlers_chat";
    
    public BamboozlersHub()
    {
        NetworkEvents.TellOthersToReadDatabaseRequest.Register(nameof(BamboozlersHub), async (senderId, chatId, dbEntry) =>
        {
            var readDatabaseS2CPacket = new ReadDatabaseS2CPacket
            {
                DbEntry = dbEntry
            };
            Console.WriteLine($"Sending packet to chat {chatId} from user {senderId}");
            await Clients.Group(chatId.ToString()).SendAsync("RecievePacketOnClient", ((IPacket)readDatabaseS2CPacket).Serialize());
            await Clients.Caller.SendAsync("RecievePacketOnClient", ((IPacket)readDatabaseS2CPacket).Serialize());
        });
    }

    public async Task JoinChat(int userId, int chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        
        Console.WriteLine($"User {userId} joined chat {chatId}");
    }

    public async Task RecievePacketOnServer(string packetJson)
    {
        await ServerNetworkHandler.Instance.HandlePacket(packetJson);
    }
    
    public override Task OnConnectedAsync()
    {
        Console.WriteLine("Client connected: " + Context.ConnectionId);
        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Client disconnected: " + Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}