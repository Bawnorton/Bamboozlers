using System.Diagnostics;
using System.Text.RegularExpressions;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserServices;

public class UserGroupService(IAuthService authService, IUserInteractionService userInteractionService, IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory) : IUserGroupService
{
    private IAuthService AuthService { get; } = authService;

    private IUserInteractionService UserInteractionService { get; } = userInteractionService;
    private IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; } = dbContextFactory;
    
    private static bool IsOwner(GroupChat groupChat, User user)
    {
        return groupChat.OwnerID == user.Id;
    }
    
    private static bool IsModerator(GroupChat groupChat, User user)
    {
        return groupChat.Moderators.FirstOrDefault(u => u.Id == user.Id) is not null;
    }
    
    private static bool IsMemberOf(Chat chat, User user)
    {
        return chat.Users.FirstOrDefault(u => u.Id == user.Id) is not null;
    }

    private static bool HasPermissions(GroupChat groupChat, User user)
    {
        return IsOwner(groupChat, user) || IsModerator(groupChat, user);
    }
    
    private static bool Outranks(User first, User second, GroupChat groupChat)
    {
        return IsOwner(groupChat, first) ||
               (IsModerator(groupChat, first) && !IsModerator(groupChat, second) && !IsOwner(groupChat,second));
    }
    
    private async Task<(User?, GroupChat?)> GetUserAndGroup(int? chatId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        
        var self = await AuthService.GetUser(query => 
            query.Include(u => u.ModeratedChats)
            .Include(u => u.OwnedChats)
            .Include(u => u.Chats)
        );
        
        var group = await dbContext.GroupChats
            .Where(gc => gc.ID == chatId)
            .Include(gc => gc.Users).ThenInclude(u => u.ModeratedChats)
            .Include(gc => gc.Users).ThenInclude(u => u.Chats)
            .Include(gc => gc.Users).ThenInclude(u => u.OwnedChats)
            .Include(gc => gc.Moderators)
            .Include(gc => gc.Owner)
            .FirstOrDefaultAsync();
        
        return (self, group);
    }
    
    public async Task<GroupInvite?> FindGroupInvite(int? chatId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
            return null;
        
        return await dbContext.GroupInvites.AsNoTracking().FirstOrDefaultAsync(
            i => i.GroupID == group.ID && i.RecipientID == self.Id
        );
    }

    public async Task AcceptGroupInvite(int? chatId)
    {
        var invite = await FindGroupInvite(chatId);
        if (invite is null) 
            return;
        
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
            return;
        
        dbContext.Remove(invite);
        
        var friendship = await UserInteractionService.FindFriendship(invite.SenderID);
        if (friendship is not null)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Database.ExecuteSqlAsync(
                $"DELETE FROM [dbo].[GroupInvites] WHERE RecipientID = {self.Id} AND SenderID = {invite.SenderID} AND GroupID = {group.ID};"
            );
            await dbContext.Database.ExecuteSqlAsync(
                $"INSERT INTO [dbo].[ChatUser] VALUES ({group.ID},{self.Id});"
            );
            await transaction.CommitAsync();
        }
        
        await dbContext.SaveChangesAsync();
        await NotifySubscribersOf(group.ID);
    }

    public async Task DeclineGroupInvite(int? chatId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null)
            return;
        
        var invite = await FindGroupInvite(chatId);
        if (invite is null) 
            return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Database.ExecuteSqlAsync(
            $"DELETE FROM [dbo].[GroupInvites] WHERE RecipientID = {self.Id} AND SenderID = {invite.SenderID} AND GroupID = {group.ID};"
        );
        await transaction.CommitAsync();
        await NotifySubscribersOf(invite.GroupID);
    }

    public async Task RemoveGroupMember(int? chatId, int? memberId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        var other = group.Users.FirstOrDefault(u => u.Id == memberId);
        if (other is null) return;

        var outranksSelf = Outranks(self, other, group);
        if (outranksSelf)
            return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        if (IsModerator(group, other))
        {
            await dbContext.Database.ExecuteSqlAsync(
                $"DELETE FROM [dbo].[GroupChatUser] WHERE ModeratedChatsID = {group.ID} AND ModeratorsId = {other.Id};"
            );
        }
        await dbContext.Database.ExecuteSqlAsync(
            $"DELETE FROM [dbo].[ChatUser] WHERE ChatsID = {group.ID} AND UsersId = {other.Id};"
        );
        await transaction.CommitAsync();
        await NotifySubscribersOf(group.ID);
    }

    public async Task AssignPermissions(int? chatId, int? modId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        var mod = group.Users.FirstOrDefault(u => u.Id == modId);
        if (mod is null) return;
        
        var selfOutranks = Outranks(self, mod, group);
        
        if (!selfOutranks || IsModerator(group, mod))
            return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Database.ExecuteSqlAsync(
            $"INSERT INTO [dbo].[GroupChatUser] VALUES ({group.ID},{mod.Id});"
        );
        await transaction.CommitAsync();
        await NotifySubscribersOf(group.ID);
    }

    public async Task RevokePermissions(int? chatId, int? modId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        var mod = group.Users.FirstOrDefault(u => u.Id == modId);
        if (mod is null) return;

        var selfOutranks = Outranks(self, mod, group);
        if (!selfOutranks || !IsModerator(group, mod))
            return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Database.ExecuteSqlAsync(
            $"DELETE FROM [dbo].[GroupChatUser] WHERE ModeratedChatsID = {group.ID} AND ModeratorsId = {mod.Id};"
        );
        await transaction.CommitAsync();
        await NotifySubscribersOf(group.ID);
    }

    public async Task SendGroupInvite(int? chatId, int? recipientId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var friendship = await UserInteractionService.FindFriendship(recipientId);
        
        if (self is null || group is null || recipientId is null || friendship is null)
            return;
        
        var invite = await FindGroupInvite(chatId);
        if (invite is null && (IsModerator(group, self) || IsOwner(group, self)))
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Database.ExecuteSqlAsync(
                $"INSERT INTO [dbo].[GroupInvites] VALUES ({self.Id},{recipientId},{group.ID},0);"
            );
            await transaction.CommitAsync();
            await NotifySubscribersOf(group.ID);
        }
    }
    
    public async Task RevokeGroupInvite(int? chatId, int? recipientId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        
        if (self is null || group is null || recipientId is null)
            return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var invite = await dbContext.GroupInvites.FirstOrDefaultAsync(
            i => i.SenderID == self.Id && i.RecipientID == recipientId && i.GroupID == group.ID    
        );
        
        if (invite is not null)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Database.ExecuteSqlAsync(
                $"DELETE FROM [dbo].[GroupInvites] WHERE RecipientID = {recipientId} AND SenderID = {self.Id} AND GroupID = {group.ID};"
            );
            await transaction.CommitAsync();
            await NotifySubscribersOf(group.ID);
        }
    }

    public async Task LeaveGroup(int? chatId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        if (self is null || group is null) return;

        Debug.WriteLine("Group and user are found");

        foreach (var user in group.Users)
        {
            Debug.WriteLine($"User in chat: {user.UserName}");
        }
        if (!IsMemberOf(group, self) || IsOwner(group, self))
            return;
        
        Debug.WriteLine("Is member and not owner");
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        if (IsModerator(group, self))
        {
            Debug.WriteLine("Is moderator");
            await dbContext.Database.ExecuteSqlAsync(
                $"DELETE FROM [dbo].[GroupChatUser] WHERE ModeratedChatsID = {group.ID} AND ModeratorsId = {self.Id};"
            );
        }
        await dbContext.Database.ExecuteSqlAsync(
            $"DELETE FROM [dbo].[ChatUser] WHERE ChatsID = {group.ID} AND UsersId = {self.Id};"
        );
        await transaction.CommitAsync();
        Debug.WriteLine("Transaction ended");
        await NotifySubscribersOf(group.ID);
    }

    public async Task<List<GroupChat>?> GetAllModeratedGroups()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser(
            query => 
                query.Include(u => u.ModeratedChats).ThenInclude(c => c.Users)
                        .Include(u => u.OwnedChats).ThenInclude(c => c.Users)
        );
        
        if (self is null) return null;
        var list = self.ModeratedChats.ToList();
        list.AddRange(self.OwnedChats.ToList());
        return list;
    }
    
    public async Task<List<GroupInvite>?> GetAllIncomingInvites()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null ? 
            dbContext.GroupInvites.AsNoTracking().Where(i => i.RecipientID == self.Id).ToList() 
            : null;
    }
    
    public async Task<List<GroupInvite>?> GetAllOutgoingInvites()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null ? dbContext.GroupInvites.AsNoTracking().Where(i => i.SenderID == self.Id).ToList() : null;
    }

    public List<IAsyncGroupSubscriber> Subscribers { get; } = [];
    
    public bool AddSubscriber(IAsyncGroupSubscriber subscriber) 
    {
        if (Subscribers.Contains(subscriber)) return false;
        Subscribers.Add(subscriber);
        return true;
    }

    private async Task NotifySubscribersOf(int groupId)
    {
        var subset = Subscribers.Where(s => s.WatchedIDs.Contains(groupId));
        foreach (var sub in subset)
        {
            await sub.OnGroupUpdate();
        }
    }
    
    public async Task NotifyAllAsync()
    {
        foreach (var sub in Subscribers)
        {
            await sub.OnGroupUpdate();
        }
    }
}

public interface IUserGroupService : IAsyncPublisher<IAsyncGroupSubscriber>
{
    Task<GroupInvite?> FindGroupInvite(int? chatId);
    Task AcceptGroupInvite(int? chatId);
    Task DeclineGroupInvite(int? chatId);
    
    Task RemoveGroupMember(int? chatId, int? memberId);

    Task AssignPermissions(int? chatId, int? modId);
    
    Task RevokePermissions(int? chatId, int? modId);
    
    Task SendGroupInvite(int? chatId, int? recipientId);
    
    Task RevokeGroupInvite(int? chatId, int? recipientId);

    Task LeaveGroup(int? chatId);
    Task<List<GroupChat>?> GetAllModeratedGroups();
    Task<List<GroupInvite>?> GetAllIncomingInvites();
    Task<List<GroupInvite>?> GetAllOutgoingInvites();
}