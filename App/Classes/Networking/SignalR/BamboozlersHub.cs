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
 * So instead we have to deal with all messages being sent to all clients, and then the client decides what to do with them
 * See the commented out code for how it would work if this tech stack wasn't a mess
 */

// ReSharper disable UnusedMember.Global - This class is used by the SignalR framework
// [Authorize]
public class BamboozlersHub : Hub
{
    public const string HubUrl = "/bamboozlers_chat";
    // private static readonly ConnectionMapping<string> Connections = new();
    
    public async Task RecievePacketOnServer(string packetJson)
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
                    // Console.WriteLine($"Sending ReadDatabaseS2CPacket to {string.Join(", ", recipientIdentityNames)}");
                    // var connections = recipientIdentityNames.Select(name => Connections.GetConnections(name)).SelectMany(connections => connections).ToList();
                    await Clients.All.SendAsync("RecievePacketOnClient", ((IPacket)readDatabaseS2CPacket).Serialize());
                    break;
            }
        });
    }
    
    public override Task OnConnectedAsync()
    {
        // var userId = Context.User!.Identity!.Name!;
        // Connections.Add(userId, Context.ConnectionId);
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // var userId = Context.User!.Identity!.Name!;
        // Connections.Remove(userId, Context.ConnectionId);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}