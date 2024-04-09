using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;

namespace Bamboozlers.Classes.Services.UserServices;

public class UserGroupService(
    IAuthService authService,
    IUserInteractionService userInteractionService,
    IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory) : IUserGroupService
{
    private IAuthService AuthService { get; } = authService;
    private IUserInteractionService UserInteractionService { get; } = userInteractionService;
    private IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; } = dbContextFactory;
    
    private async Task<bool> Outranks(int? firstId, int? secondId, int? chatId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var groupChat = await dbContext.GroupChats.FirstOrDefaultAsync(gc => gc.ID == chatId);
        if (groupChat is null) return false;
        
        if (groupChat.OwnerID == firstId) return true;
        if (groupChat.OwnerID == secondId) return false;

        var firstIsMod = await dbContext.ChatModerators.FirstOrDefaultAsync(cm => cm.GroupChatId == chatId && cm.UserId == firstId) is not null;
        var secondIsMod = await dbContext.ChatModerators.FirstOrDefaultAsync(cm => cm.GroupChatId == chatId && cm.UserId == secondId) is not null;
        return firstIsMod && !secondIsMod;
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

    private async Task FindSuccessorOwner(int? chatId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        var group = await dbContext.GroupChats.FirstOrDefaultAsync(g => g.ID == chatId);
        if (group is null) return;

        var modList = dbContext.ChatModerators.Where(cm => cm.GroupChatId == chatId)
            .Select(cm => cm.UserId)
            .ToList();
        var chatUsers = dbContext.ChatUsers.Where(cu => cu.ChatId == chatId)
            .OrderBy(cu => cu.JoinDate)
            .ToList();
        var eligibleSuccessors = chatUsers.Where(u => modList.Contains(u.UserId))
            .OrderBy(cu => cu.JoinDate)
            .ToList();
        eligibleSuccessors.AddRange(chatUsers.Except(eligibleSuccessors));
        
        var successor = eligibleSuccessors.FirstOrDefault();
        if (successor is not null)
        {
            group.OwnerID = successor.UserId;
            var modEntry = await dbContext.ChatModerators.FirstOrDefaultAsync(cm => cm.GroupChatId == chatId && cm.UserId == successor.UserId);
            if (modEntry is not null)
            {
                dbContext.ChatModerators.Remove(modEntry);
            }
            await dbContext.SaveChangesAsync();
            return;
        }

        await DeleteGroup(chatId, true);
    }

    public async Task<IdentityResult> CreateGroup(byte[]? avatar = null, string? name = null, List<User>? sendInvites = null)
    {
        var self = await AuthService.GetUser(query => 
                query.Include(u => u.OwnedChats)
                    .Include(u => u.Chats)
        );
        if (self is null) return IdentityResult.Failed();

        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var entityEntry = dbContext.GroupChats.Add(new GroupChat(self.Id));
        await dbContext.SaveChangesAsync();
        
        var newChatId = entityEntry.Entity.ID;
        var group = dbContext.GroupChats.First(gc => gc.ID == newChatId);
        group.Name = name.IsNullOrEmpty() ? null : name;
        group.Avatar = avatar;
        await dbContext.SaveChangesAsync();
        
        dbContext.ChatUsers.Add(new ChatUser(self.Id, group.ID));
        await dbContext.SaveChangesAsync();

        if (sendInvites is not null && sendInvites.Count > 0)
        {
            foreach (var invite in sendInvites.Select(other => new GroupInvite(self.Id, other.Id, group.ID)))
            {
                dbContext.GroupInvites.Add(invite);
                await dbContext.SaveChangesAsync();
            }
        }

        await NotifySubscribersOf(-1, GroupEvent.CreateGroup);
        return IdentityResult.Success;
    }
    
    public async Task<IdentityResult> DeleteGroup(int? chatId, bool overridePerms = false)
    {
        if (!overridePerms)
        {
            var (self, group) = await GetUserAndGroup(chatId);
            if (self is null || group is null) return IdentityResult.Failed();
        
            if (group.OwnerID != self.Id) return IdentityResult.Failed();
        }
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var grp = dbContext.GroupChats.First(gc => gc.ID == chatId);
        foreach (var chatUser in dbContext.ChatUsers.Where(cu => cu.ChatId == chatId).ToList())
        {
            dbContext.ChatUsers.Remove(chatUser);
        }
        foreach (var chatModerator in dbContext.ChatModerators.Where(cm => cm.GroupChatId == chatId).ToList())
        {
            dbContext.ChatModerators.Remove(chatModerator);
        }
        dbContext.Remove(grp);
        await dbContext.SaveChangesAsync();

        await NotifySubscribersOf(-1, GroupEvent.DeleteGroup);
        return IdentityResult.Success;
    }
    
    public async Task<IdentityResult> UpdateGroupAvatar(int? chatId, byte[]? avatar)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return IdentityResult.Failed();
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var isMod = await dbContext.ChatModerators.FirstOrDefaultAsync(cm 
            => cm.GroupChatId == chatId && cm.UserId == self.Id
        ) is not null;
        
        if (!isMod && group.OwnerID != self.Id)
        {
            return IdentityResult.Failed([new IdentityError{Description = "Issue occurred that prevented group changes from being saved."}]);
        }
        
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
        if (self is null || group is null) return IdentityResult.Failed();
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var isMod = await dbContext.ChatModerators.FirstOrDefaultAsync(cm 
            => cm.GroupChatId == chatId && cm.UserId == self.Id
        ) is not null;
        
        if (!isMod && group.OwnerID != self.Id)
        {
            return IdentityResult.Failed([new IdentityError{Description = "Issue occurred that prevented group changes from being saved."}]);
        }
        
        group = dbContext.GroupChats.Include(gc => gc.Owner)
                    .First(gc => gc.ID == chatId);
        group.Name = name.IsNullOrEmpty() ? null : name;
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(group.ID, GroupEvent.GroupDisplayChange);

        return IdentityResult.Success;
    }

    public async Task<GroupInvite?> FindIncomingGroupInvite(int? chatId, int? senderId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
        {
            return null;
        }

        return await dbContext.GroupInvites.AsNoTracking().FirstOrDefaultAsync(
            i => i.GroupID == group.ID && i.RecipientID == self.Id && i.SenderID == senderId
        );
    }
    
    public async Task<GroupInvite?> FindOutgoingGroupInvite(int? chatId, int? recipientId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null)
        {
            return null;
        }

        return await dbContext.GroupInvites.AsNoTracking().FirstOrDefaultAsync(
            i => i.GroupID == group.ID && i.RecipientID == recipientId && i.SenderID == self.Id
        );
    }

    public async Task AcceptGroupInvite(int? chatId, int? senderId)
    {
        var invite = await FindIncomingGroupInvite(chatId, senderId);
        if (invite is null) return;

        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (self is null || group is null) return;

        dbContext.GroupInvites.Remove(invite);
        dbContext.ChatUsers.Add(new ChatUser(self.Id, (int)chatId!));
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(-1, GroupEvent.ReceivedInviteAccepted);
    }

    public async Task DeclineGroupInvite(int? chatId, int? senderId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;

        var invite = await FindIncomingGroupInvite(chatId, senderId);
        if (invite is null) return;

        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        dbContext.GroupInvites.Remove(invite);
        await dbContext.SaveChangesAsync();
        await NotifySubscribersOf(-1, GroupEvent.ReceivedInviteDeclined);
    }

    public async Task RemoveGroupMember(int? chatId, int? memberId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        
        var outranks = await Outranks(self.Id, memberId, chatId);
        if (!outranks) return;

        var chatUser = await dbContext.ChatUsers.FirstOrDefaultAsync(
            cu => cu.ChatId == chatId && cu.UserId == memberId
        );
        var chatMod = await dbContext.ChatModerators.FirstOrDefaultAsync(
            cm => cm.GroupChatId == chatId && cm.UserId == memberId
        );
        
        if (chatUser is not null) 
            dbContext.ChatUsers.Remove(chatUser);
        if (chatMod is not null)
            dbContext.ChatModerators.Remove(chatMod);
        await dbContext.SaveChangesAsync();

        await NotifySubscribersOf(group.ID, GroupEvent.RemoveMember);
    }

    public async Task AssignPermissions(int? chatId, int? modId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        
        var selfOutranks = await Outranks(self.Id, modId, chatId);
        var chatMod = await dbContext.ChatModerators
            .FirstOrDefaultAsync(cm => cm.GroupChatId == chatId && cm.UserId == modId);
        if (!selfOutranks || chatMod is not null) return;
        
        dbContext.ChatModerators.Add(new ChatModerator((int) modId!, (int) chatId!));
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(group.ID, GroupEvent.GrantedPermissions);
    }

    public async Task RevokePermissions(int? chatId, int? modId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        if (self is null || group is null) return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        var selfOutranks = await Outranks(self.Id, modId, chatId);
        if (!selfOutranks) return;

        var chatMod = await dbContext.ChatModerators
            .FirstOrDefaultAsync(cm => cm.GroupChatId == chatId && cm.UserId == modId);
        if (chatMod is not null)
        {
            dbContext.ChatModerators.Remove(chatMod);
            await dbContext.SaveChangesAsync();
        }

        var invitesToClear = dbContext.GroupInvites.Where(i => i.SenderID == modId && i.GroupID == chatId).ToList();
        foreach (var inv in invitesToClear)
        {
            dbContext.GroupInvites.Remove(inv);
        }
        await dbContext.SaveChangesAsync();

        await NotifySubscribersOf(group.ID, GroupEvent.RevokedPermissions);
    }

    public async Task SendGroupInvite(int? chatId, int? recipientId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var friendship = await UserInteractionService.FindFriendship(recipientId);

        if (self is null || group is null || friendship is null) return;

        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == recipientId);
        if (other is null) return;
        
        var invite = await FindOutgoingGroupInvite(chatId,recipientId);
        var isMod = await dbContext.ChatModerators.FirstOrDefaultAsync(cm 
            => cm.GroupChatId == chatId && cm.UserId == self.Id
        ) is not null;
        if (invite is null && (group.OwnerID == self.Id || isMod))
        {
            invite = new GroupInvite(self.Id, other.Id, group.ID);
            dbContext.GroupInvites.Add(invite);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(-1, GroupEvent.SentInvite);
        }
    }

    public async Task RevokeGroupInvite(int? chatId, int? recipientId)
    {
        var (self, group) = await GetUserAndGroup(chatId);

        if (self is null || group is null || recipientId is null) return;

        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var invite = await dbContext.GroupInvites.FirstOrDefaultAsync(
            i => i.SenderID == self.Id && i.RecipientID == recipientId && i.GroupID == group.ID
        );

        if (invite is not null)
        {
            dbContext.Remove(invite);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(-1,GroupEvent.SentInviteRevoked);
        }
    }

    public async Task LeaveGroup(int? chatId)
    {
        var (self, group) = await GetUserAndGroup(chatId);
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        if (self is null || group is null) return;

        var chatUser = await dbContext.ChatUsers.FirstOrDefaultAsync(
            cu => cu.ChatId == chatId && cu.UserId == self.Id
        );
        var chatMod = await dbContext.ChatModerators.FirstOrDefaultAsync(
            cm => cm.GroupChatId == chatId && cm.UserId == self.Id
        );
        
        if (chatUser is not null) 
            dbContext.ChatUsers.Remove(chatUser);
        if (chatMod is not null)
            dbContext.ChatModerators.Remove(chatMod);
        await dbContext.SaveChangesAsync();
        
        if (self.Id == group.OwnerID)
        {
            await FindSuccessorOwner(chatId);
        }
        
        await NotifySubscribersOf(-1, GroupEvent.SelfLeftGroup);
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
        if (self is null) return [];
        
        return dbContext.GroupInvites.AsNoTracking()
            .Where(i => i.RecipientID == self.Id)
            .Include(i => i.Sender)
            .Include(i => i.Recipient)
            .Include(i => i.Group)
            .ThenInclude(g => g.Owner)
            .ToList();
    }

    public async Task<List<GroupInvite>> GetAllOutgoingInvites()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        if (self is null) return [];

        return dbContext.GroupInvites.AsNoTracking()
            .Where(i => i.SenderID == self.Id)
            .Include(i => i.Sender)
            .Include(i => i.Recipient)
            .Include(i => i.Group)
            .ThenInclude(g => g.Owner)
            .ToList();
    }

    public List<IGroupSubscriber> Subscribers { get; } = [];
    
    public bool AddSubscriber(IGroupSubscriber subscriber) 
    {
        if (Subscribers.Contains(subscriber)) return false;
        Subscribers.Add(subscriber);
        return true;
    }

    public async Task NotifySubscribersOf(int groupId, GroupEvent evt)
    {
        var subscribersToGroup = groupId == -1 ? Subscribers : Subscribers.Where(s => s.WatchedIDs.Contains(groupId)).ToList();
        if (evt is GroupEvent.General)
        {
            foreach (var sub in subscribersToGroup)
            {
                await sub.OnUpdate(GroupEvent.General);
            }
        }
        else
        {
            subscribersToGroup = subscribersToGroup.Where(s => s.WatchedGroupEvents.Contains(evt) || s.WatchedGroupEvents.Contains(GroupEvent.General)).ToList();
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
    Task<IdentityResult> DeleteGroup(int? chatId, bool overridePerms = false);
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
    Task NotifySubscribersOf(int groupId, GroupEvent evt);
}