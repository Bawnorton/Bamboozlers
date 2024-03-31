using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace Tests.Provider.MockAppDbContext;

public class MockAppDbContext
{
    public Mock<AppDbContext> MockDbContext { get; set; }
    public MockUsers MockUsers { get; set; }
    public MockChats MockChats { get; set; }
    public MockMessages MockMessages { get; set; }
    public MockBlocks MockBlocks { get; set; }
    public MockFriendRequests MockFriendRequests { get; set; }
    public MockFriendships MockFriendships{ get; set; }
    public MockGroupInvites MockGroupInvites { get; set; }

    public MockAppDbContext(Mock<AppDbContext> _mockDbContext)
    {
        MockDbContext = _mockDbContext;

        MockUsers = new MockUsers(this);
        MockChats = new MockChats(this, MockUsers.mockUsers.Object);
        MockMessages = new MockMessages(this, MockUsers.mockUsers.Object, MockChats.mockChats.Object);
        MockBlocks = new MockBlocks(this, MockUsers.mockUsers.Object);
        MockFriendRequests = new MockFriendRequests(this, MockUsers.mockUsers.Object);
        MockFriendships = new MockFriendships(this, MockUsers.mockUsers.Object);
        MockGroupInvites = new MockGroupInvites(this, MockUsers.mockUsers.Object, MockChats.mockChats.Object);
    }
    
    private int FindEntryIndex<T>(T entry,
        List<T> entriesList,
        Func<T, T, bool> matchPredicate) where T : class
    {
        var entryMatch = entriesList.FirstOrDefault(e => matchPredicate.Invoke(e,entry));
        return entryMatch is not null 
            ? entriesList.IndexOf(entryMatch)
            : -1;
    }
    
    public Mock<DbSet<T>> AddMockDbEntry<T>(T entry, 
        Mock<DbSet<T>> entries, 
        Func<T,T,bool> matchPredicate) where T : class
    {
        var entriesList = entries.Object.ToList();
        var matchIdx = FindEntryIndex(entry, entriesList, matchPredicate);
        if (matchIdx != -1)
        {
            entriesList[matchIdx] = entry;
        }
        else
        {
            entriesList.Add(entry);
        }
        entries = SetupMockDbSet(entriesList);
        return entries;
    }
    
    public Mock<DbSet<T>> RemoveMockDbEntry<T>(T entry, 
        Mock<DbSet<T>> entries, 
        Func<T,T,bool> matchPredicate) where T : class
    {
        var entriesList = entries.Object.ToList();
        var matchIdx = FindEntryIndex(entry, entriesList, matchPredicate);

        if (matchIdx != -1)
        {
            entriesList.RemoveAt(matchIdx);
        }
        entries = SetupMockDbSet(entriesList);
        return entries;
    }
    
    public Mock<DbSet<T>> SetupMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        return data.AsQueryable().BuildMockDbSet();
    }
}