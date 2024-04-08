using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Networking.SignalR;


// ReSharper disable UnusedMember.Global - This class is used by the SignalR framework
public class BamboozlersHub(IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory)
    : Hub
{
    private IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; set; } = dbContextFactory;

    public const string HubUrl = "/bamboozlers_hub";
    
    private static readonly ConnectionMapping<int> Connections = new();

    public async Task ReceivePacketOnServer(string packetJson)
    {
        await ServerNetworkHandler.Instance.Handle(packetJson, async packet =>
        {
            Console.WriteLine($"Recieved packet from client: {packet.PacketType().GetId()}");
            switch (packet)
            {
                case JoinChatC2SPacket joinChat:
                    await Groups.AddToGroupAsync(Context.ConnectionId, joinChat.ChatId.ToString());
                    break;
                case MessageSentC2SPacket messageSent:
                    var messageSentResponse = new MessageSentS2CPacket
                    {
                        MessageId = messageSent.MessageId
                    };
                    await SendToChat(messageSent.ChatId, messageSentResponse);
                    break;
                case MessageEditedC2SPacket messageEdited:
                    var messageEditedResponse = new MessageEditedS2CPacket
                    {
                        MessageId = messageEdited.MessageId,
                        NewContent = messageEdited.NewContent
                    };
                    await SendToChat(messageEdited.ChatId, messageEditedResponse);
                    break;
                case MessageDeletedC2SPacket messageDeleted:
                    var messageDeletedRespone = new MessageDeletedS2CPacket
                    {
                        MessageId = messageDeleted.MessageId
                    };
                    await SendToChat(messageDeleted.ChatId, messageDeletedRespone);
                    break;
                case MessagePinStatusC2SPacket messagePinStatus:
                    var messagePinStatusResponse = new MessagePinStatusS2CPacket
                    {
                        MessageId = messagePinStatus.MessageId,
                        Pinned = messagePinStatus.Pinned
                    };
                    await SendToChat(messagePinStatus.ChatId, messagePinStatusResponse);
                    break;
                case InteractionSyncC2SPacket userRelation:
                    var userRelationResponse = new InteractionSyncS2CPacket
                    {
                        Event = userRelation.Event
                    };
                    await SendToUser(userRelation.ReceiverId, userRelationResponse);
                    break;
            }
        });
    }
    
    private async Task SendToChat(int chatId, IPacket packet)
    {
        Console.WriteLine($"Sending packet to chat {chatId}: {packet.PacketType().GetId()}");
        await Clients.Groups(chatId.ToString()).SendAsync("RecievePacketOnClient", packet.Serialize());
    }
    
    private async Task SendToUser(int userId, IPacket packet)
    {
        var success = false;
        foreach (var connectionId in Connections.GetConnections(userId))
        {
            Console.WriteLine($"Sending packet to user {userId} on connection {connectionId}: {packet.PacketType().GetId()}");
            success = true;
            await Clients.Client(connectionId).SendAsync("RecievePacketOnClient", packet.Serialize());
        }
        if (!success)
        {
            Console.WriteLine($"Failed to send packet to user {userId}: {packet.PacketType().GetId()} (No connections)");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var name = Context.User!.Identity?.Name;
        if (name is null)
        {
            Context.Abort();
            Console.WriteLine($"Client failed to connect: {Context.ConnectionId} (No name)");
            return;
        }
        await using var db = await DbContextFactory.CreateDbContextAsync();
        var users = db.Users.AsQueryable().ToList();
        var user = users.Find(u => u.UserName == name);
        if (user is null)
        {
            Context.Abort();
            Console.WriteLine($"Client failed to connect: {Context.ConnectionId} ({name}) Not in database");
            return;
        }
        Console.WriteLine($"Client connected: {Context.ConnectionId} ({name}) with id {user.Id}");
        Connections.Add(user.Id, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var name = Context.User!.Identity?.Name;
        if (name is null)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId} (No name)");
            Connections.RemoveConnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
            return;
        }
        await using var db = await DbContextFactory.CreateDbContextAsync();
        var users = db.Users.AsQueryable().ToList();
        var user = users.Find(u => u.UserName == name);
        if (user is not null)
        {
            Connections.Remove(user.Id, Context.ConnectionId);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId} ({name}) with id {user.Id}");
            await base.OnDisconnectedAsync(exception);
            return;
        }
        Console.WriteLine($"Client disconnected: {Context.ConnectionId} ({name}) Not in database");
        Connections.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}