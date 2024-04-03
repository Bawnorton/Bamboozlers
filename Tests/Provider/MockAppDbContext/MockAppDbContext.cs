using System.Diagnostics.CodeAnalysis;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace Tests.Provider.MockAppDbContext;

// API Class
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class MockAppDbContext
{
    public MockAppDbContext(Mock<AppDbContext> mockDbContext)
    {
        MockDbContext = mockDbContext;

        MockUsers = new MockUsers(this);
        MockChats = new MockChats(this, MockUsers.MockUsersSet.Object);
        MockMessages = new MockMessages(this, MockUsers.MockUsersSet.Object, MockChats.MockChatSet.Object);
        MockBlocks = new MockBlocks(this, MockUsers.MockUsersSet.Object);
        MockFriendRequests = new MockFriendRequests(this, MockUsers.MockUsersSet.Object);
        MockFriendships = new MockFriendships(this, MockUsers.MockUsersSet.Object);
        MockGroupInvites = new MockGroupInvites(this);
    }

    public Mock<AppDbContext> MockDbContext { get; set; }
    public MockUsers MockUsers { get; set; }
    public MockChats MockChats { get; set; }
    public MockMessages MockMessages { get; set; }
    public MockBlocks MockBlocks { get; set; }
    public MockFriendRequests MockFriendRequests { get; set; }
    public MockFriendships MockFriendships { get; set; }
    public MockGroupInvites MockGroupInvites { get; set; }

    private int FindEntryIndex<T>(T entry,
        List<T> entriesList,
        Func<T, T, bool> matchPredicate) where T : class
    {
        var entryMatch = entriesList.FirstOrDefault(e => matchPredicate.Invoke(e, entry));
        return entryMatch is not null
            ? entriesList.IndexOf(entryMatch)
            : -1;
    }

    public Mock<DbSet<T>> AddMockDbEntry<T>(T entry,
        Mock<DbSet<T>> entries,
        Func<T, T, bool> matchPredicate) where T : class
    {
        var entriesList = entries.Object.ToList();
        var matchIdx = FindEntryIndex(entry, entriesList, matchPredicate);
        if (matchIdx != -1)
            entriesList[matchIdx] = entry;
        else
            entriesList.Add(entry);
        entries = SetupMockDbSet(entriesList);
        return entries;
    }

    public Mock<DbSet<T>> RemoveMockDbEntry<T>(T entry,
        Mock<DbSet<T>> entries,
        Func<T, T, bool> matchPredicate) where T : class
    {
        var entriesList = entries.Object.ToList();
        var matchIdx = FindEntryIndex(entry, entriesList, matchPredicate);

        if (matchIdx != -1) entriesList.RemoveAt(matchIdx);
        entries = SetupMockDbSet(entriesList);
        return entries;
    }

    public Mock<DbSet<T>> SetupMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        return data.AsQueryable().BuildMockDbSet();
    }
}