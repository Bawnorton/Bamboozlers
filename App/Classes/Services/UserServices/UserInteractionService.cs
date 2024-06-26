using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserServices;

public class UserInteractionService(
    IAuthService authService,
    IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory) : IUserInteractionService
{
    private IAuthService AuthService { get; } = authService;
    private IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; } = dbContextFactory;

    private async Task<(User?, User?)> GetInvolvedUsers(int? otherId, Unary<IQueryable<User>>? inclusionCallback = null)
    {        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser(inclusionCallback);
        
        var query = dbContext.Users.AsQueryable();
        query = inclusionCallback?.Invoke(query) ?? query;
        var other = await query.FirstOrDefaultAsync(u => u.Id == otherId);
        
        return (self, other);
    }
    
    public async Task<Friendship?> FindFriendship(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return null;

        return await dbContext.FriendShips.FirstOrDefaultAsync(f =>
            (f.User1ID == other.Id && f.User2ID == self.Id)
            || (f.User1ID == self.Id && f.User2ID == other.Id)
        );
    }

    public async Task<(FriendRequest?, FriendRequest?)> FindFriendRequests(int? otherId)
    {
        var incoming = await FindIncomingRequest(otherId);
        var outgoing = await FindOutgoingRequest(otherId);
        return (incoming, outgoing);
    }
    
    public async Task<FriendRequest?> FindIncomingRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return null;

        return await dbContext.FriendRequests.FirstOrDefaultAsync(r =>
            r.ReceiverID == self.Id
            && r.SenderID == other.Id
        );
    }

    public async Task<FriendRequest?> FindOutgoingRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return null;

        return await dbContext.FriendRequests.FirstOrDefaultAsync(r =>
            r.ReceiverID == other.Id
            && r.SenderID == self.Id
        );
    }

    public async Task<Block?> FindIfBlocked(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return null;

        return await dbContext.BlockList.FirstOrDefaultAsync(r =>
            r.BlockerID == self.Id
            && r.BlockedID == other.Id
        );
    }

    public async Task<Block?> FindIfBlockedBy(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return null;

        return await dbContext.BlockList.FirstOrDefaultAsync(r =>
            r.BlockedID == self.Id
            && r.BlockerID == other.Id
        );
    }

    public async Task<Chat?> FindDm(int? otherId)
    {
        var friendship = await FindFriendship(otherId);
        if (friendship is null) return null;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(
            otherId, 
            query => query
                .Include(u => u.Chats)
                .ThenInclude(c => c.Users));

        var existingChat = self!.Chats.Where(c => c is not GroupChat)
            .FirstOrDefault(c => c.Users.FirstOrDefault(u => u.Id == self.Id) is not null
                                 && c.Users.FirstOrDefault(u => u.Id == other!.Id) is not null);

        return existingChat;
    }
    
    public async Task<Chat?> CreateDm(int? otherId)
    {
        var chatExists = await FindDm(otherId);
        if (chatExists is not null) return chatExists;
        
        var friendship = await FindFriendship(otherId);
        if (friendship is null) return null;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(
            otherId,
            query => query.Include(u => u.Chats)
        );

        if (self is null || other is null) return null;

        var entityEntry = await dbContext.Chats.AddAsync(new Chat());
        await dbContext.SaveChangesAsync();
        var newChatId = entityEntry.Entity.ID;

        var chatUser0 = new ChatUser(self.Id, newChatId);
        var chatUser1 = new ChatUser(other.Id, newChatId);
        dbContext.ChatUsers.AddRange([chatUser0,chatUser1]);
        await dbContext.SaveChangesAsync();
        
        await NotifySubscribersOf(InteractionEvent.CreateDm);

        return await dbContext.Chats.FirstOrDefaultAsync(c => c.ID == newChatId);
    }
    
    public async Task BlockUser(int? otherId)
    {
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return;

        var blockEntry = await FindIfBlocked(otherId);

        if (blockEntry is null)
        {
            await using var dbContext = await DbContextFactory.CreateDbContextAsync();

            var (incoming, outgoing) = await FindFriendRequests(otherId);

            if (incoming is not null) dbContext.FriendRequests.Remove(incoming);
            if (outgoing is not null) dbContext.FriendRequests.Remove(outgoing);

            var friendship = await FindFriendship(otherId);
            if (friendship is not null)
                dbContext.Remove(friendship);
            var block = new Block(other.Id, self.Id);
            await dbContext.BlockList.AddAsync(block);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(InteractionEvent.Block);
        }
    }

    public async Task UnblockUser(int? otherId)
    {
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null)
            return;

        var blockEntry = await FindIfBlocked(otherId);

        if (blockEntry is not null)
        {
            await using var dbContext = await DbContextFactory.CreateDbContextAsync();

            dbContext.BlockList.Remove(blockEntry);

            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(InteractionEvent.Unblock);
        }
    }

    public async Task SendFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return;

        var (incoming, outgoing) = await FindFriendRequests(otherId);

        if (outgoing is null && incoming is null)
        {
            var friendRequest = new FriendRequest(self.Id, other.Id);
            await dbContext.FriendRequests.AddAsync(friendRequest);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(InteractionEvent.RequestSent);
        }
    }

    public async Task RevokeFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return;

        var requestEntry = await FindOutgoingRequest(otherId);

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(InteractionEvent.RequestRevoked);
        }
    }

    public async Task AcceptFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null) return;

        var requestEntry = await FindIncomingRequest(otherId);

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            var friendship = new Friendship(self.Id, other.Id);
            await dbContext.FriendShips.AddAsync(friendship);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(InteractionEvent.RequestDeclined);
        }
    }

    public async Task DeclineFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == otherId);

        if (self is null || other is null) return;

        var requestEntry = await FindIncomingRequest(otherId);

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(InteractionEvent.RequestDeclined);
        }
    }

    public async Task RemoveFriend(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == otherId);

        if (self is null || other is null) return;

        var friendship = await FindFriendship(otherId);

        if (friendship is not null)
        {
            dbContext.Remove(friendship);
            await dbContext.SaveChangesAsync();
            await NotifySubscribersOf(InteractionEvent.Unfriend);
        }
    }

    public async Task<List<FriendRequest>> GetAllIncomingRequests()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        if (self is null) return [];

        return dbContext.FriendRequests.AsNoTracking()
            .Where(r => r.ReceiverID == self.Id)
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .ToList(); 
    }

    public async Task<List<FriendRequest>> GetAllOutgoingRequests()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        if (self is null) return [];
        
        return dbContext.FriendRequests.AsNoTracking()
                .Where(r => r.SenderID == self.Id)
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .ToList();
    }

    public async Task<List<User>> GetAllFriends()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        if (self is null) return [];
        
        return dbContext.FriendShips.AsNoTracking()
            .Where(f => f.User1ID == self.Id || f.User2ID == self.Id)
            .Select(s => s.User1ID != self.Id ? s.User1 : s.User2).ToList();
    }
    
    public async Task<List<User>> GetAllBlocked()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        if (self is null) return [];
        
        return dbContext.BlockList.AsNoTracking()
            .Where(f => f.BlockerID == self.Id)
            .Select(s => s.Blocked).ToList();
    }
    
    public async Task<List<User>> GetAllBlockedBy()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        if (self is null) return [];
        
        return dbContext.BlockList.AsNoTracking()
            .Where(f => f.BlockedID == self.Id)
            .Select(s => s.Blocker).ToList();
    }

    public List<IInteractionSubscriber> Subscribers { get; } = [];
    
    public Task NotifySubscribersOf(InteractionEvent evt)
    {
        lock (Subscribers)
        {
            if (evt is InteractionEvent.General)
            {
                foreach (var sub in Subscribers.ToList())
                {
                    sub.OnUpdate(InteractionEvent.General);
                }
            }
            else
            {
                foreach (var sub in Subscribers.Where(s => s.WatchedInteractionEvents.Contains(evt) || s.WatchedInteractionEvents.Contains(InteractionEvent.General)).ToList())
                {
                    sub.OnUpdate(evt);
                }
            }
            return Task.CompletedTask;
        }
    }
    public bool AddSubscriber(IInteractionSubscriber subscriber) 
    {
        lock (Subscribers)
        {
            if (Subscribers.Contains(subscriber)) return false;
            Subscribers.Add(subscriber);
            return true;
        }
    }

    public Task NotifyAllAsync()
    {
        lock (Subscribers)
        {
            foreach (var sub in Subscribers)
            {
                sub.OnUpdate(InteractionEvent.General);
            }
            return Task.CompletedTask;
        }
    }
}

public interface IUserInteractionService : IAsyncPublisher<IInteractionSubscriber>
{
    Task<Friendship?> FindFriendship(int? otherId);
    Task<FriendRequest?> FindIncomingRequest(int? otherId);
    Task<FriendRequest?> FindOutgoingRequest(int? otherId);
    Task<Block?> FindIfBlocked(int? otherId);
    Task<Block?> FindIfBlockedBy(int? otherId);
    Task<Chat?> FindDm(int? otherId);
    Task<Chat?> CreateDm(int? otherId);
    Task BlockUser(int? otherId);
    Task UnblockUser(int? otherId);
    Task SendFriendRequest(int? otherId);
    Task RevokeFriendRequest(int? otherId);
    Task AcceptFriendRequest(int? otherId);
    Task DeclineFriendRequest(int? otherId);
    Task RemoveFriend(int? otherId);
    Task<List<FriendRequest>> GetAllIncomingRequests();
    Task<List<FriendRequest>> GetAllOutgoingRequests();
    Task<List<User>> GetAllFriends();
    Task<List<User>> GetAllBlockedBy();
    Task<List<User>> GetAllBlocked();
    Task NotifySubscribersOf(InteractionEvent evt);
}