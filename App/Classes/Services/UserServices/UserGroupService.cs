using System.Diagnostics;
using System.Text.RegularExpressions;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Utility.Observer;
using Blazorise;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

    public async Task<IdentityResult> CreateGroup(byte[]? avatar = null, string? name = null, List<User>? sendInvites = null)
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
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var entityEntry = dbContext.GroupChats.Add(new GroupChat(self.Id));
        await dbContext.SaveChangesAsync();
        var newChatId = entityEntry.Entity.ID;
        dbContext.ChangeTracker.Clear();

        var group = dbContext.GroupChats.First(gc => gc.ID == newChatId);
        dbContext.AttachRange([self, group]);
        
        group.Name = name.IsNullOrEmpty() ? $"{self.UserName}'s Group" : name!;
        group.Avatar = avatar;
        
        group.Users = [self];
        self.OwnedChats.Add(group);
        self.Chats.Add(group);

        dbContext.GroupChats.Update(group);
        await dbContext.SaveChangesAsync();

        if (sendInvites is not null && sendInvites.Count > 0)
        {
            foreach (var invite in sendInvites.Select(other => new GroupInvite(self.Id, other.Id, group.ID)))
            {
                await dbContext.GroupInvites.AddAsync(invite);
                await dbContext.SaveChangesAsync();
            }
        }

        await NotifyAllAsync();
        return IdentityResult.Success;
    }

    private static bool Exists(User? self, GroupChat? group)
    {
        return self is not null && group is not null;
    }

    private static bool HasPerms(User self, GroupChat group)
    {
        return IsModerator(group, self) || IsOwner(group, self);
    }
    
    private static bool ExistsAndHasPerms(User? self, GroupChat? group)
    {
        return Exists(self, group) && HasPerms(self!, group!);
    }
    
    public async Task<IdentityResult> UpdateGroupAvatar(int? chatId, byte[]? avatar)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        
        if (!ExistsAndHasPerms(self,group))
            return IdentityResult.Failed([new IdentityError{Description = "Issue occurred that prevented group changes from being saved."}]);
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        group = dbContext.GroupChats.First(gc => gc.ID == chatId);
        avatar = avatar.IsNullOrEmpty() ? null : avatar;
        
        group.Avatar = avatar;
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(group.ID, GroupEvent.GroupDisplayChange);
        return IdentityResult.Success;
    }
    
    public async Task<IdentityResult> UpdateGroupName(int? chatId, string? name)
    {
        var (self, group) = await GetUserAndGroup(chatId);

        if (!ExistsAndHasPerms(self,group))
            return IdentityResult.Failed([new IdentityError{Description = "Issue occurred that prevented group changes from being saved."}]);
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        group = dbContext.GroupChats.Include(gc => gc.Owner)
                    .First(gc => gc.ID == chatId);
        name = name.IsNullOrEmpty() ? $"{group.Owner.UserName}'s Group" : name;
        group.Name = name!;
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(group.ID, GroupEvent.GroupDisplayChange);
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
        
        self.Chats.Add(group);
        group.Users.Add(self);
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(group.ID, GroupEvent.ReceivedInviteAccepted);
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
        await NotifySubscribersOf(group.ID, GroupEvent.ReceivedInviteDeclined);
    }

    public async Task RemoveGroupMember(int? chatId, int? memberId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null)
            return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var other = await dbContext.Users.Where(u => u.Id == memberId)
            .Include(u => u.Chats)
            .Include(u => u.ModeratedChats)
            .FirstOrDefaultAsync();
        
        if (other is null || !IsMemberOf(group, other)) return;

        var skipChecks = other.Id == self.Id;
        if (!skipChecks)
        {
            if (!Outranks(self, other, group))
                return;
        }
        
        if (IsModerator(group, other))
        {
            other.ModeratedChats.Remove(group);
            group.Moderators.Remove(other);
        }
        other.Chats.Remove(group);
        group.Users.Remove(other);
        await dbContext.SaveChangesAsync();
        await NotifySubscribersOf(group.ID, GroupEvent.RemoveMember);
    }

    public async Task AssignPermissions(int? chatId, int? modId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var mod = await dbContext.Users.Where(u => u.Id == modId)
            .Include(u => u.Chats)
            .Include(u => u.ModeratedChats)
            .FirstOrDefaultAsync();
        if (mod is null || !IsMemberOf(group, mod)) return;
        
        var selfOutranks = Outranks(self, mod, group);
        
        if (!selfOutranks || IsModerator(group, mod))
            return;
        
        group.Moderators.Add(mod);
        mod.ModeratedChats.Add(group);
        await dbContext.SaveChangesAsync();
        await NotifySubscribersOf(group.ID, GroupEvent.GrantedPermissions);
    }

    public async Task RevokePermissions(int? chatId, int? modId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var mod = await dbContext.Users.Where(u => u.Id == modId)
            .Include(u => u.Chats)
            .Include(u => u.ModeratedChats)
            .FirstOrDefaultAsync();
        if (mod is null || !IsMemberOf(group, mod)) return;

        var selfOutranks = Outranks(self, mod, group);
        if (!selfOutranks || !IsModerator(group, mod))
            return;
        
        group.Moderators.Remove(mod);
        mod.ModeratedChats.Remove(group);
        await dbContext.SaveChangesAsync();
        await NotifySubscribersOf(group.ID, GroupEvent.RevokedPermissions);
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
            await NotifySubscribersOf(group.ID, GroupEvent.SentInvite);
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
            await NotifySubscribersOf(group.ID,GroupEvent.SentInviteRevoked);
        }
    }

    public async Task LeaveGroup(int? chatId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        if (self is null || group is null) return;

        await RemoveGroupMember(chatId, self.Id);
        await NotifySubscribersOf(group.ID, GroupEvent.SelfLeftGroup);
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
            dbContext.GroupInvites.AsNoTracking()
                .Where(i => i.RecipientID == self.Id)
                    .Include(i => i.Sender)
                        .Include(i => i.Recipient)
                            .Include(i => i.Group)
                                .ToList() 
            : [];
    }
    
    public async Task<List<GroupInvite>> GetAllOutgoingInvites()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null 
            ? dbContext.GroupInvites.AsNoTracking()
                .Where(i => i.SenderID == self.Id)
                    .Include(i => i.Sender)
                        .Include(i => i.Recipient)
                            .Include(i => i.Group)
                                .ToList()
            : [];
    }

    public List<IGroupSubscriber> Subscribers { get; } = [];
    
    public bool AddSubscriber(IGroupSubscriber subscriber) 
    {
        if (Subscribers.Contains(subscriber)) return false;
        Subscribers.Add(subscriber);
        return true;
    }

    private async Task NotifySubscribersOf(int groupId, GroupEvent evt)
    {
        var subscribersToGroup = groupId == -1 ? Subscribers : Subscribers.Where(s => s.WatchedIDs.Contains(groupId));
        if (evt is GroupEvent.General)
        {
            foreach (var sub in subscribersToGroup)
            {
                await sub.OnUpdate(GroupEvent.General);
            }
        }
        else
        {
            subscribersToGroup = subscribersToGroup.Where(s => s.WatchedGroupEvents.Contains(evt));
            foreach (var sub in subscribersToGroup)
            {
                await sub.OnUpdate(evt);
            }
        }
    }
    
    public async Task NotifyAllAsync()
    {
        foreach (var sub in Subscribers)
        {
            await sub.OnUpdate(GroupEvent.General);
        }
    }
}

public interface IUserGroupService : IAsyncPublisher<IGroupSubscriber>
{
    Task<IdentityResult> CreateGroup(byte[]? avatar = null, string? name = null, List<User>? sendInvites = null);
    Task<IdentityResult> UpdateGroupAvatar(int? chatId, byte[]? avatar);
    Task<IdentityResult> UpdateGroupName(int? chatId, string? name);
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