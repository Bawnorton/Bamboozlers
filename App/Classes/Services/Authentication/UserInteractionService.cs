using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services.Authentication;

public class UserInteractionService(IAuthService authService, IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory) : IUserInteractionService
{
    private IAuthService AuthService { get; set; } = authService;
    private IDbContextFactory<AppDbContext.AppDbContext> DbContextFactory { get; set; } = dbContextFactory;

    public async Task<Friendship?> FindFriendship(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);

        if (self is null || other is null)
            return null;
        
        return await dbContext.FriendShips.FirstOrDefaultAsync(f =>
            (f.User1ID == other.Id && f.User2ID == self.Id)
            || (f.User1ID == self.Id && f.User2ID == other.Id)
        );
    }
    
    public async Task<FriendRequest?> FindIncomingRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);

        if (self is null || other is null)
            return null;
        
        return await dbContext.FriendRequests.FirstOrDefaultAsync(r =>
            r.ReceiverID == self.Id 
            && r.SenderID == other.Id 
            && r.Status == RequestStatus.Pending
        );
    }

    public async Task<FriendRequest?> FindOutgoingRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);

        if (self is null || other is null)
            return null;
        
        return await dbContext.FriendRequests.FirstOrDefaultAsync(r =>
            r.ReceiverID == other.Id 
            && r.SenderID == self.Id 
            && r.Status == RequestStatus.Pending
        );
    }

    public async Task<Block?> FindIfBlocked(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);

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
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);

        if (self is null || other is null)
            return null;
        
        return await dbContext.BlockList.FirstOrDefaultAsync(r =>
            r.BlockedID == self.Id 
            && r.BlockerID == other.Id
        );
    }

    public async Task BlockUser(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var blockEntry = await dbContext.BlockList.FirstOrDefaultAsync(
            b => b.BlockerID == self.Id && b.BlockedID == otherId
        );
        
        if (blockEntry is null)
        {
            var outgoing = await FindOutgoingRequest(otherId);
            var incoming = await FindIncomingRequest(otherId);

            if (outgoing is not null) dbContext.FriendRequests.Remove(outgoing);
            if (incoming is not null) dbContext.FriendRequests.Remove(incoming);
            
            blockEntry = new Block
            {
                Blocker = self,
                Blocked = other,
            };
            await dbContext.BlockList.AddAsync(blockEntry);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task UnblockUser(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var blockEntry = await dbContext.BlockList.FirstOrDefaultAsync(
            b => b.BlockerID == self.Id && b.BlockedID == otherId
        );

        if (blockEntry is not null)
        {
            dbContext.BlockList.Remove(blockEntry);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task SendFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var outgoing = await FindOutgoingRequest(otherId);
        var incoming = await FindIncomingRequest(otherId);

        if (outgoing is null && incoming is null)
        {
            var requestEntry = new FriendRequest
            {
                Sender = self,
                Receiver = other
            };
            await dbContext.FriendRequests.AddAsync(requestEntry);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task RevokeFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var requestEntry = await dbContext.FriendRequests.FirstOrDefaultAsync(
            f => f.SenderID == self.Id && f.ReceiverID == other.Id
        );

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task AcceptFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var requestEntry = await dbContext.FriendRequests.FirstOrDefaultAsync(
            f => f.SenderID == self.Id && f.ReceiverID == other.Id
        );

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            var friendEntry = new Friendship
            {
                User1 = self,
                User2 = other,
            };
            await dbContext.FriendShips.AddAsync(friendEntry);
            await dbContext.SaveChangesAsync();
        }
    }
    
    public async Task DeclineFriendRequest(int? otherId)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var self = await AuthService.GetUser();
        var other = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == otherId);
        
        if (self is null || other is null)
            return;

        var requestEntry = await FindIncomingRequest(otherId);

        if (requestEntry is not null)
        {
            dbContext.FriendRequests.Remove(requestEntry);
            await dbContext.SaveChangesAsync();
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
        }
    }
}

public interface IUserInteractionService
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
}