using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Clientbound.Messaging;
using Bamboozlers.Classes.Networking.Packets.Clientbound.User;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Chat;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Interaction;
using Bamboozlers.Classes.Networking.Packets.Serverbound.Messaging;
using Bamboozlers.Classes.Networking.Packets.Serverbound.User;
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
            switch (packet)
            {
                case JoinChatC2SPacket joinChat:
                    await Groups.AddToGroupAsync(Context.ConnectionId, joinChat.ChatId.ToString());
                    break;
                case LeaveChatC2SPacket leaveChat:
                    var updateTypingState = new TypingStateS2CPacket
                    {
                        Typing = false,
                        UserId = leaveChat.SenderId
                    };
                    await SendToChat(leaveChat.ChatId, updateTypingState);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, leaveChat.ChatId.ToString());
                    break;
                case TypingStateC2SPacket typingState:
                    var typingStateResponse = new TypingStateS2CPacket
                    {
                        Typing = typingState.Typing,
                        UserId = typingState.SenderId,
                        ChatId = typingState.ChatId
                    };
                    await SendToChat(typingState.ChatId, typingStateResponse);
                    break;
                case MessageSentC2SPacket messageSent:
                    var messageSentResponse = new MessageSentS2CPacket
                    {
                        MessageId = messageSent.MessageId,
                        ChatId = messageSent.ChatId
                    };
                    await SendToChat(messageSent.ChatId, messageSentResponse);
                    break;
                case MessageEditedC2SPacket messageEdited:
                    var messageEditedResponse = new MessageEditedS2CPacket
                    {
                        MessageId = messageEdited.MessageId,
                        NewContent = messageEdited.NewContent,
                        ChatId = messageEdited.ChatId
                    };
                    await SendToChat(messageEdited.ChatId, messageEditedResponse);
                    break;
                case MessageDeletedC2SPacket messageDeleted:
                    var messageDeletedRespone = new MessageDeletedS2CPacket
                    {
                        MessageId = messageDeleted.MessageId,
                        ChatId = messageDeleted.ChatId
                    };
                    await SendToChat(messageDeleted.ChatId, messageDeletedRespone);
                    break;
                case MessagePinStatusC2SPacket messagePinStatus:
                    var messagePinStatusResponse = new MessagePinStatusS2CPacket
                    {
                        MessageId = messagePinStatus.MessageId,
                        Pinned = messagePinStatus.Pinned,
                        ChatId = messagePinStatus.ChatId
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
                    if (groupInteractionSync.Event == GroupEvent.GroupDisplayChange)
                    {
                        await using var db = await DbContextFactory.CreateDbContextAsync();
                        var invites = db.GroupInvites
                            .AsQueryable()
                            .ToList()
                            .Where(i => i.GroupID == groupInteractionSync.GroupId)
                            .SelectMany(i => new[] {i.SenderID, i.RecipientID});
                        invites = invites.Distinct().ToList();
                        foreach (var id in invites)
                        {
                            await SendToUser(id, groupInteractionSyncResponse);
                        }
                    }
                    break;
                case UserDataSyncC2SPacket userDataSync:
                {
                    await using var db = await DbContextFactory.CreateDbContextAsync();
                    var userId = userDataSync.UserId;
                    
                    var user = db.Users
                        .AsQueryable()
                        .ToList()
                        .Find(u => u.Id == userId);
                    if (user == null)
                    {
                        Console.WriteLine($"User {userId} ({userDataSync.Username}) not found in database");
                        return;
                    }

                    if (user.UserName != userDataSync.Username)
                    {
                        Context.Abort();
                    }
                    
                    var friends = db.FriendShips
                        .AsQueryable()
                        .ToList()
                        .Where(f => f.User1ID == userId || f.User2ID == userId)
                        .Select(f => f.User1ID == userId ? f.User2ID : f.User1ID);
                    var potentialFriends = db.FriendRequests
                        .AsQueryable()
                        .ToList()
                        .Where(f => f.ReceiverID == userId)
                        .Select(f => f.SenderID);
                    potentialFriends = potentialFriends.Concat(db.FriendRequests
                        .AsQueryable()
                        .ToList()
                        .Where(f => f.SenderID == userId)
                        .Select(f => f.ReceiverID));
                    var chatUsers = db.Chats
                        .Include(c => c.Users)
                        .AsQueryable()
                        .ToList()
                        .Where(c => c.Users.Any(u => u.Id == userId))
                        .SelectMany(c => c.Users)
                        .Select(u => u.Id);
                    
                    var toSend = new List<int>();
                    toSend.AddRange(friends);
                    toSend.AddRange(potentialFriends);
                    toSend.AddRange(chatUsers);
                    toSend = toSend.Distinct().ToList();
                    
                    var response = new UserDataSyncS2CPacket
                    {
                        UserId = userId
                    };
                    foreach (var id in toSend)
                    {
                        await SendToUser(id, response);
                    }
                    break;
                }
                case UserDeletedC2SPacket userDeleted:
                {
                    var toNotify = userDeleted.ToNotify;
                    var response = new UserDataSyncS2CPacket
                    {
                        UserId = userDeleted.UserId
                    };
                    foreach (var id in toNotify)
                    {
                        await SendToUser(id, response);
                    }
                    Context.Abort();
                    break;
                }
            }
        });
    }
    
    private async Task SendToChat(int chatId, IPacket packet)
    {
        await Clients.Group(chatId.ToString()).SendAsync("RecievePacketOnClient", packet.Serialize());
    }
    
    private async Task SendToUser(int userId, IPacket packet)
    {
        foreach (var connectionId in Connections.GetConnections(userId))
        {
            await Clients.Client(connectionId).SendAsync("RecievePacketOnClient", packet.Serialize());
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