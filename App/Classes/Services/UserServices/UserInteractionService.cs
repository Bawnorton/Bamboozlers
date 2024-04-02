using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.UserServices;

public class UserInteractionService(IAuthService authService, IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory) : IUserInteractionService
{
    private IAuthService AuthService { get; set; } = authService;
    private IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; set; } = dbContextFactory;

    private async Task<(User?, User?)> GetInvolvedUsers(int? otherId, Unary<IQueryable<User>>? inclusionCallback = null)
    {        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser(inclusionCallback);
        var other = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == otherId);
        return (self, other);
    }
    
    public async Task<Friendship?> FindFriendship(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null)
            return null;
        
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

        if (self is null || other is null)
            return null;
        
        return await dbContext.FriendRequests.FirstOrDefaultAsync(r =>
            r.ReceiverID == self.Id 
            && r.SenderID == other.Id 
        );
    }

    public async Task<FriendRequest?> FindOutgoingRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null)
            return null;
        
        return await dbContext.FriendRequests.FirstOrDefaultAsync(r =>
            r.ReceiverID == other.Id 
            && r.SenderID == self.Id 
        );
    }
    
    public async Task<Block?> FindIfBlocked(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null)
            return null;
        
        return await dbContext.BlockList.FirstOrDefaultAsync(r =>
            r.BlockerID == self.Id 
            && r.BlockedID == other.Id
        );
    }

    public async Task<Block?> FindIfBlockedBy(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);

        if (self is null || other is null)
            return null;
        
        return await dbContext.BlockList.FirstOrDefaultAsync(r =>
            r.BlockedID == self.Id 
            && r.BlockerID == other.Id
        );
    }

    public async Task BlockUser(int? otherId)
    {
        var (self, other) = await GetInvolvedUsers(otherId);
        
        if (self is null || other is null)
            return;

        var blockEntry = await FindIfBlocked(otherId);
        
        if (blockEntry is null)
        {
            await using var dbContext = await DbContextFactory.CreateDbContextAsync();
            
            var (incoming,outgoing) = await FindFriendRequests(otherId);
            
            if (incoming is not null) dbContext.FriendRequests.Remove(incoming);
            if (outgoing is not null) dbContext.FriendRequests.Remove(outgoing);

            var block = new Block(other.Id, self.Id);
            await dbContext.BlockList.AddAsync(block);
            await dbContext.SaveChangesAsync();
            await NotifyAllAsync();
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
            await NotifyAllAsync();
        }
    }

    public async Task SendFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);
        
        if (self is null || other is null)
            return;
        
        var (incoming,outgoing) = await FindFriendRequests(otherId);
        
        if (outgoing is null && incoming is null)
        {
            var friendRequest = new FriendRequest(self.Id, other.Id);
            await dbContext.FriendRequests.AddAsync(friendRequest);
            await dbContext.SaveChangesAsync();
            await NotifyAllAsync();
        }
    }

    public async Task RevokeFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);
        
        if (self is null || other is null)
            return;

        var requestEntry = await FindOutgoingRequest(otherId);

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            await dbContext.SaveChangesAsync();
            await NotifyAllAsync();
        }
    }

    public async Task AcceptFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var (self, other) = await GetInvolvedUsers(otherId);
        
        if (self is null || other is null)
            return;

        var requestEntry = await FindIncomingRequest(otherId);
        
        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            var friendship = new Friendship(self.Id, other.Id);
            await dbContext.FriendShips.AddAsync(friendship);
            await dbContext.SaveChangesAsync();
            await NotifyAllAsync();
        }
    }
    
    public async Task DeclineFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var requestEntry = await FindIncomingRequest(otherId);

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            await dbContext.SaveChangesAsync();
            await NotifyAllAsync();
        }
    }

    public async Task RemoveFriend(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var friendship = await FindFriendship(otherId);

        if (friendship is not null)
        {
            dbContext.Remove(friendship);
            await dbContext.SaveChangesAsync();
            await NotifyAllAsync();
        }
    }

    public async Task<List<FriendRequest>> GetAllIncomingRequests()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null 
            ? dbContext.FriendRequests.AsNoTracking()
                .Where(r => r.ReceiverID == self.Id)
                    .Include(r => r.Sender)
                        .Include(r => r.Receiver)
                            .ToList() 
            : [];
    }

    public async Task<List<FriendRequest>> GetAllOutgoingRequests()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null
            ? dbContext.FriendRequests.AsNoTracking()
                .Where(r => r.SenderID == self.Id)
                    .Include(r => r.Sender)
                        .Include(r => r.Receiver)
                            .ToList() 
            : [];
    }

    public async Task<List<User>> GetAllFriends()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null
            ? dbContext.FriendShips.AsNoTracking()
                .Where(f => f.User1ID == self.Id || f.User2ID == self.Id)
                .Select(s => s.User1ID != self.Id ? s.User1 : s.User2).ToList()
            : [];
    }
    
    public async Task<List<User>> GetAllBlocked()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null
            ? dbContext.BlockList.AsNoTracking()
                .Where(f => f.BlockerID == self.Id)
                .Select(s => s.Blocked).ToList()
            : [];
    }
    
    public async Task<List<User>> GetAllBlockedBy()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        return self is not null
            ? dbContext.BlockList.AsNoTracking()
                .Where(f => f.BlockedID == self.Id)
                .Select(s => s.Blocker).ToList()
            : [];
    }

    public List<IAsyncInteractionSubscriber> Subscribers { get; } = [];
    
    public bool AddSubscriber(IAsyncInteractionSubscriber subscriber) 
    {
        if (Subscribers.Contains(subscriber)) return false;
        Subscribers.Add(subscriber);
        
        subscriber.OnInteractionUpdate();
        
        return true;
    }
    
    public async Task NotifyAllAsync()
    {
        foreach (var sub in Subscribers)
        {
            await sub.OnInteractionUpdate();
        }
    }
}

public interface IUserInteractionService : IAsyncPublisher<IAsyncInteractionSubscriber>
{
    Task<Friendship?> FindFriendship(int? otherId);
    Task<FriendRequest?> FindIncomingRequest(int? otherId);
    Task<FriendRequest?> FindOutgoingRequest(int? otherId);
    Task<Block?> FindIfBlocked(int? otherId);
    Task<Block?> FindIfBlockedBy(int? otherId);
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
}