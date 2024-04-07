using Bamboozlers.Classes.AppDbContext;

namespace Tests.InteractionTests;

public class InteractionTestBase : AuthenticatedBlazoriseTestBase
{
    public (List<User>, List<Friendship>, List<FriendRequest>, List<Block>) BuildTestCases()
    {
        MockDatabaseProvider.GetMockAppDbContext().MockUsers.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.ClearAll();
        
        List<User> users = [];
        for (var i = 0; i < 10; i++)
        {
            users.Add(MockUserManager.CreateMockUser(i));
        }

        var friendships = new List<Friendship>
        {
            new (0,1)
            {
                User1 = users[0],
                User2 = users[1]
            },
            new (0,2)
            {
                User1 = users[0],
                User2 = users[2]
            }
        };
        foreach (var friendship in friendships)
        {
            MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship);
        }

        var friendRequests = new List<FriendRequest>
        {
            new(0, 3)
            {
                Receiver = users[3],
                Sender = users[0],
                Status = RequestStatus.Pending
            },
            new(0, 4)
            {
                Receiver = users[4],
                Sender = users[0],
                Status = RequestStatus.Pending
            },
            new(5, 0)
            {
                Receiver = users[0],
                Sender = users[5],
                Status = RequestStatus.Pending
            },
            new(6, 0)
            {
                Receiver = users[0],
                Sender = users[6],
                Status = RequestStatus.Pending
            },
        };
        foreach (var friendRequest in friendRequests)
        {
            MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.AddMock(friendRequest);
        }

        var blocks = new List<Block>
        {
            new (7, 0)
            {
                Blocked = users[7],
                Blocker = users[0]
            },
            new (0, 8)
            {
                Blocked = users[0],
                Blocker = users[8]
            },
        };
        foreach (var block in blocks)
        {
            MockDatabaseProvider.GetMockAppDbContext().MockBlocks.AddMock(block);
        }
        
        return (users, friendships, friendRequests, blocks);
    }
}