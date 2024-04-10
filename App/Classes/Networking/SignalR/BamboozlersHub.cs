using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;
using Bamboozlers.Classes.Utility.Observer;
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
                    var didJoinChat = new DidJoinChatS2CPacket
                    {
                        ChatId = joinChat.ChatId
                    };
                    await SendToUser(joinChat.SenderId, didJoinChat);
                    break;
                case LeaveChatC2SPacket leaveChat:
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, leaveChat.ChatId.ToString());
                    var updateTypingState = new TypingStateS2CPacket
                    {
                        Typing = false,
                        UserId = leaveChat.SenderId
                    };
                    await SendToChat(leaveChat.ChatId, updateTypingState);
                    var didLeaveChat = new DidLeaveChatS2CPacket
                    {
                        ChatId = leaveChat.ChatId
                    };
                    await SendToUser(leaveChat.SenderId, didLeaveChat);
                    break;
                case TypingStateC2SPacket typingState:
                    var typingStateResponse = new TypingStateS2CPacket
                    {
                        Typing = typingState.Typing,
                        UserId = typingState.SenderId
                    };
                    await SendToChat(typingState.ChatId, typingStateResponse);
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
                case InteractionSyncC2SPacket interactionSync:
                    var responseEvent = interactionSync.Event switch
                    {
                        InteractionEvent.RequestSent => InteractionEvent.RequestReceived,
                        _ => interactionSync.Event
                    };
                    var interactionSyncResponse = new InteractionSyncS2CPacket
                    {
                        Event = responseEvent
                    };
                    await SendToUser(interactionSync.ReceiverId, interactionSyncResponse);
                    break;
                case GroupInteractionSyncC2SPacket groupInteractionSync:
                    var responseGroupEvent = groupInteractionSync.Event switch
                    {
                        GroupEvent.SelfLeftGroup => GroupEvent.OtherLeftGroup,
                        GroupEvent.SentInvite => GroupEvent.ReceivedInvite,
                        GroupEvent.SentInviteRevoked => GroupEvent.ReceivedInviteRevoked,
                        GroupEvent.RevokedPermissions => GroupEvent.PermissionsLost,
                        GroupEvent.GrantedPermissions => GroupEvent.PermissionsGained,
                        _ => groupInteractionSync.Event
                    };
                    var groupInteractionSyncResponse = new GroupInteractionSyncS2CPacket
                    {
                        Event = responseGroupEvent,
                        SpecificUserId = groupInteractionSync.SpecificUserId,
                        GroupId = groupInteractionSync.GroupId
                    };
                    await SendToChat(groupInteractionSync.GroupId, groupInteractionSyncResponse);
                    
                    if (groupInteractionSync.SpecificUserId != -1)
                    {
                        await SendToUser(groupInteractionSync.SpecificUserId, groupInteractionSyncResponse);
                    }
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