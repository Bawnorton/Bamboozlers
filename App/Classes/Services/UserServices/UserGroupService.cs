using System.Diagnostics;
using System.Text.RegularExpressions;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Utility.Observer;
using Blazorise;
using Microsoft.AspNetCore.Identity;
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

    public async Task<IdentityResult> CreateGroup(byte[]? avatar = null, string? name = null)
    {
        var self = await AuthService.GetUser(
            query => 
                query.Include(u => u.OwnedChats)
                    .Include(u => u.Chats)
            );
        if (self is null)
            return IdentityResult.Failed([
                new IdentityError { Description = "Could not create group. Database error occured." }
            ]);
        
        var group = new GroupChat
        {
            Owner = self,
            OwnerID = self.Id,
            Name = name ?? $"{self.UserName}'s Group",
            Avatar = avatar,
            Users = [self],
            Moderators = [],
            Messages = []
        };
        self.OwnedChats.Add(group);
        self.Chats.Add(group);

        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        dbContext.Attach(self);
        dbContext.Attach(group);
        await dbContext.SaveChangesAsync();
        
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateGroupDisplay(int? chatId, byte[]? avatar = null, string? name = null)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
            return IdentityResult.Failed([new IdentityError{Description ="Could not save group changes. Database error occurred."}]);

        if (!IsModerator(group, self) && !IsOwner(group, self))
            return IdentityResult.Failed([new IdentityError{Description ="Could not save group changes. You do not have permissions to do so."}]);
        
        name ??= $"{group.Owner.UserName}'s Group";
        
        group.Avatar = avatar;
        group.Name = name;
        dbContext.Attach(group);
        await dbContext.SaveChangesAsync();
        
        return IdentityResult.Success;
    }

    public async Task<GroupInvite?> FindIncomingGroupInvite(int? chatId, int? senderId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
            return null;
        
        return await dbContext.GroupInvites.AsNoTracking().FirstOrDefaultAsync(
            i => i.GroupID == group.ID && i.RecipientID == self.Id && i.SenderID == senderId
        );
    }
    
    public async Task<GroupInvite?> FindOutgoingGroupInvite(int? chatId, int? recipientId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
            return null;
        
        return await dbContext.GroupInvites.AsNoTracking().FirstOrDefaultAsync(
            i => i.GroupID == group.ID && i.RecipientID == recipientId && i.SenderID == self.Id
        );
    }

    public async Task AcceptGroupInvite(int? chatId, int? senderId)
    {
        var invite = await FindIncomingGroupInvite(chatId, senderId);
        if (invite is null) 
            return;
        
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
            return;
        
        dbContext.GroupInvites.Remove(invite);

        dbContext.Attach(group);
        dbContext.Attach(self);
        self.Chats.Add(group);
        group.Users.Add(self);
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(group.ID);
    }

    public async Task DeclineGroupInvite(int? chatId, int? senderId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null)
            return;
        
        var invite = await FindIncomingGroupInvite(chatId, senderId);
        if (invite is null) 
            return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        dbContext.GroupInvites.Remove(invite);
        await dbContext.SaveChangesAsync();
        await NotifySubscribersOf(invite.GroupID);
    }

    public async Task RemoveGroupMember(int? chatId, int? memberId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null)
            return;
        
        var other = group.Users.FirstOrDefault(u => u.Id == memberId);
        if (other is null)
            return;

        var skipChecks = other.Id == self.Id;
        if (!skipChecks)
        {
            if (!Outranks(self, other, group))
                return;
        }
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        dbContext.Attach(group);
        dbContext.Attach(other);
        if (IsModerator(group, other))
        {
            other.ModeratedChats.Remove(group);
            group.Moderators.Remove(other);
        }
        other.Chats.Remove(group);
        group.Users.Remove(other);
        await dbContext.SaveChangesAsync();
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
        dbContext.Attach(group);
        dbContext.Attach(mod);
        group.Moderators.Add(mod);
        mod.ModeratedChats.Add(group);
        await dbContext.SaveChangesAsync();
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
        dbContext.Attach(group);
        dbContext.Attach(mod);
        group.Moderators.Remove(mod);
        mod.ModeratedChats.Remove(group);
        await dbContext.SaveChangesAsync();
        await NotifySubscribersOf(group.ID);
    }

    public async Task SendGroupInvite(int? chatId, int? recipientId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var friendship = await UserInteractionService.FindFriendship(recipientId);
        
        if (self is null || group is null || friendship is null)
            return;

        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == recipientId);
        if (other is null)
            return;
        
        var invite = await FindOutgoingGroupInvite(chatId,recipientId);
        if (invite is null && (IsModerator(group, self) || IsOwner(group, self)))
        {
            invite = new GroupInvite(self.Id, other.Id, group.ID);
            await dbContext.GroupInvites.AddAsync(invite);
            await dbContext.SaveChangesAsync();
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
            dbContext.Remove(invite);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(group.ID);
        }
    }

    public async Task LeaveGroup(int? chatId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        if (self is null || group is null) return;

        await RemoveGroupMember(chatId, self.Id);
    }

    public async Task<List<GroupChat>> GetAllModeratedGroups()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser(
            query => 
                query.Include(u => u.ModeratedChats).ThenInclude(c => c.Users)
                        .Include(u => u.OwnedChats).ThenInclude(c => c.Users)
        );
        
        if (self is null) return [];
        var list = self.ModeratedChats.ToList();
        list.AddRange(self.OwnedChats.ToList());
        return list;
    }
    
    public async Task<List<GroupInvite>> GetAllIncomingInvites()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null ? 
            dbContext.GroupInvites.AsNoTracking().Where(i => i.RecipientID == self.Id).ToList() 
            : [];
    }
    
    public async Task<List<GroupInvite>> GetAllOutgoingInvites()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null 
            ? dbContext.GroupInvites.AsNoTracking().Where(i => i.SenderID == self.Id).ToList() 
            : [];
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
    Task<IdentityResult> CreateGroup(byte[]? avatar = null, string? name = null);
    Task<IdentityResult> UpdateGroupDisplay(int? chatId, byte[]? avatar = null, string? name = null);
    Task<GroupInvite?> FindIncomingGroupInvite(int? chatId, int? senderId);
    Task<GroupInvite?> FindOutgoingGroupInvite(int? chatId, int? senderId);
    Task AcceptGroupInvite(int? chatId, int? senderId);
    Task DeclineGroupInvite(int? chatId, int? senderId);
    Task RemoveGroupMember(int? chatId, int? memberId);
    Task AssignPermissions(int? chatId, int? modId);
    Task RevokePermissions(int? chatId, int? modId);
    Task SendGroupInvite(int? chatId, int? recipientId);
    Task RevokeGroupInvite(int? chatId, int? recipientId);
    Task LeaveGroup(int? chatId);
    Task<List<GroupChat>> GetAllModeratedGroups();
    Task<List<GroupInvite>> GetAllIncomingInvites();
    Task<List<GroupInvite>> GetAllOutgoingInvites();
}