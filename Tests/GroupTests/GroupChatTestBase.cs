using Bamboozlers.Classes.AppDbContext;

namespace Tests.GroupTests;

public class GroupChatTestBase : AuthenticatedBlazoriseTestBase
{
    protected (List<User>,List<Friendship>,List<GroupChat>,List<GroupInvite>) BuildGroupTestCases()
    {
        MockDatabaseProvider.GetMockAppDbContext().MockChats.ClearAll(); 
        MockDatabaseProvider.GetMockAppDbContext().MockGroupInvites.ClearAll();
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.ClearAll();
        
        List<User> testUsers = [];
        List<Friendship> testFriendships = [];
        List<GroupInvite> testInvites = [];
        
        for (var i = 0; i <= 8; i++)
        {
            var user = MockUserManager.CreateMockUser(i);
            testUsers.Add(user);
        }

        for (var i = 1; i <= 4; i++)
        {
            var friendship1 = new Friendship(testUsers[0].Id,testUsers[i].Id)
            {
                User1 = testUsers[0],
                User2 = testUsers[i]
            };
            var friendship2 = new Friendship(testUsers[8].Id,testUsers[8-i].Id)
            {
                User1 = testUsers[8],
                User2 = testUsers[8-i]
            };
            testFriendships.AddRange([friendship1,friendship2]);
            MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship1);
            MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship2);
        }
        
        var groupChat1 = new GroupChat(testUsers[0].Id)
        {
            ID = 1,
            Owner = testUsers[0],
            Users = [
                testUsers[0],
                testUsers[1],
                testUsers[2]
            ],
            Moderators = [
                testUsers[1]
            ]
        };
        
        var groupChat2 = new GroupChat(testUsers[8].Id)
        {
            ID = 2,
            Owner = testUsers[8],
            Users = [
                testUsers[6],
                testUsers[7],
                testUsers[8]
            ],
            Moderators = [
                testUsers[7]
            ]
        };

        var groupChat3 = new GroupChat(testUsers[4].Id)
        {
            ID = 3,
            Name = "NamedGroup",
            Avatar = new byte[1],
            Owner = testUsers[4],
            Users =
            [
                testUsers[4],
                testUsers[0]
            ],
            Moderators =
            [
                testUsers[0]
            ],
        };
        
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(groupChat1);
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(groupChat2);
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(groupChat3);
        
        testInvites.AddRange(new []
        {
            new GroupInvite(testUsers[0].Id,testUsers[4].Id, groupChat1.ID)
            {
                Recipient = testUsers[4],
                Sender = testUsers[0],
                Group = groupChat1
            },
            new(testUsers[0].Id,testUsers[5].Id, groupChat1.ID)
            {
                Recipient = testUsers[5],
                Sender = testUsers[0],
                Group = groupChat1
            },
            new(testUsers[0].Id,testUsers[6].Id, groupChat1.ID)
            {
                Recipient = testUsers[6],
                Sender = testUsers[0],
                Group = groupChat1
            },
            new(testUsers[4].Id,testUsers[0].Id, groupChat1.ID)
            {
                Recipient = testUsers[0],
                Sender = testUsers[4],
                Group = groupChat2
            },
            new(testUsers[5].Id,testUsers[0].Id, groupChat1.ID)
            {
                Recipient = testUsers[0],
                Sender = testUsers[5],
                Group = groupChat2
            },
            new(testUsers[6].Id,testUsers[0].Id, groupChat1.ID)
            {
                Recipient = testUsers[0],
                Sender = testUsers[6],
                Group = groupChat2
            },
        });
        foreach (var invite in testInvites)
        {
            MockDatabaseProvider.GetMockAppDbContext().MockGroupInvites.AddMock(invite);
        }
        
        return (
            testUsers, 
            testFriendships, 
            [groupChat1, groupChat2, groupChat3], 
            testInvites
        );
    }
}