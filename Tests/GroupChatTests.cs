using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Blazorise.Modules;
using Bunit.Extensions.WaitForHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;
using Xunit.Abstractions;

namespace Tests;

using Bamboozlers.Components.Chat;

public class GroupChatTests : AuthenticatedBlazoriseTestBase
{
    private ITestOutputHelper output;
    public GroupChatTests(ITestOutputHelper outputHelper)
    {
        output = outputHelper;
        //MockDatabaseProvider.SetupMockDbContext();
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());
        
        _ = new MockJsRuntimeProvider(Ctx);
    }
    
    private async Task<(List<User>,List<Friendship>,List<GroupChat>,List<GroupInvite>)> BuildTestCases()
    {
        List<User> testUsers = [];
        List<Friendship> testFriendships = [];
        
        for (var i = 0; i <= 8; i++)
        {
            var user = MockUserManager.CreateMockUser(i);
            testUsers.Add(user);
        }

        for (var i = 1; i <= 4; i++)
        {
            var friendship1 = new Friendship
            {
                User1 = testUsers[0],
                User2 = testUsers[i],
                User1ID = testUsers[0].Id,
                User2ID = testUsers[i].Id,
            };
            var friendship2 = new Friendship
            {
                User1 = testUsers[8],
                User2 = testUsers[8-i],
                User1ID = testUsers[8].Id,
                User2ID = testUsers[8-i].Id,
            };
            testFriendships.AddRange([friendship1,friendship2]);
            MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship1);
            MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(friendship2);
        }
        
        var groupChat1 = new GroupChat
        {
            ID = 1,
            Owner = testUsers[0],
            OwnerID = testUsers[0].Id,
            Users = [
                testUsers[0],
                testUsers[1],
                testUsers[2]
            ],
            Moderators = [
                testUsers[1]
            ]
        };
        
        var groupChat2 = new GroupChat
        {
            ID = 2,
            Owner = testUsers[8],
            OwnerID = testUsers[8].Id,
            Users = [
                testUsers[6],
                testUsers[7],
                testUsers[8]
            ],
            Moderators = [
                testUsers[7]
            ]
        };
        
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(groupChat1);
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(groupChat2);

        var invite = new GroupInvite
        {
            RecipientID = testUsers[4].Id,
            Recipient = testUsers[4],
            SenderID = testUsers[0].Id,
            Sender = testUsers[0],
            GroupID = groupChat1.ID,
            Group = groupChat1
        };
        MockDatabaseProvider.GetMockAppDbContext().MockGroupInvites.AddMock(invite);
        
        return (
            testUsers, 
            testFriendships, 
            [groupChat1, groupChat2], 
            [invite]
        );
    }
    
    private async Task<bool> CompAddMember_CheckInviteListEntry(User self,
        User friend, 
        Chat chat,
        IEnumerable<GroupInvite> invites,
        IRenderedFragment fragment)
    {
        try
        {
            var friendSection = fragment.WaitForElement($"#{friend.UserName}-display");
            var inGroup = chat.Users.FirstOrDefault(u => u.Id == friend.Id) is not null;
            var invited = invites.FirstOrDefault(i 
                => i.SenderID == self.Id && i.RecipientID == friend.Id && i.GroupID == chat.ID) is not null;
            var actionButton = fragment.WaitForElement($"#{friend.UserName}-action-button");
                
            if (inGroup)
            {
                Assert.Contains("Already in group",actionButton.TextContent);
            }
            else if (invited)
            {
                Assert.Contains("Revoke Invitation",actionButton.TextContent);
            }
            else
            {
                Assert.Contains("Invite",actionButton.TextContent);
            }
        }
        catch (WaitForFailedException)
        {
            return false;
        }
        return true;
    }
    
    [Fact]
    public async void GroupChatTests_CompAddMember()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = await BuildTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();
        
        var component = Ctx.RenderComponent<CompAddMember>(
            parameters 
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        // Assert: Check observed the observed Chats for component
        Assert.Equal(subjectGroup.ID, component.Instance.WatchedIDs[0]);
        component.Render();
        
        // Assert: Check that the proper users are being displayed (friends) with proper options
        var friends = testFriendships.Where(f => f.User1ID == subjectUser.Id || f.User2ID == subjectUser.Id)
            .Select(s => s.User1ID == subjectUser.Id ? s.User2 : s.User1)
            .ToList();
        
        foreach (var friend in friends)
        {
            Assert.True(await CompAddMember_CheckInviteListEntry(subjectUser, friend, subjectGroup, testInvites, component));
        }
        
        // Arrange & Act: User is not found/authenticated
        await SetUser(null);
        UserService.Invalidate();
        
        component = Ctx.RenderComponent<CompAddMember>(
            parameters 
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        // Assert: Assert that the details are no longer visible
        foreach (var friend in friends)
        {
            Assert.False(await CompAddMember_CheckInviteListEntry(subjectUser, friend, subjectGroup, testInvites, component));
        }
        
        // Arrange & Act: Chat is not found/does not exist
        await SetUser(null);
        UserService.Invalidate();
        
        component.SetParametersAndRender(
            parameters 
                => parameters.Add(p => p.ChatID, -1)
        );
        
        // Assert: Assert that the details are no longer visible
        foreach (var friend in friends)
        { 
            Assert.False(await CompAddMember_CheckInviteListEntry(subjectUser, friend, subjectGroup, testInvites, component));
        }
        
        // Arrange & Act: Test Observer Pattern
        subjectUser = testUsers[8];
        subjectGroup = testGroups[1];
        
        await SetUser(subjectUser);
        UserService.Invalidate();
        
        component.SetParametersAndRender(
            parameters 
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        friends = testFriendships.Where(f => f.User1ID == subjectUser.Id || f.User2ID == subjectUser.Id)
            .Select(s => s.User1ID == subjectUser.Id ? s.User2 : s.User1)
            .ToList();
        
        output.WriteLine(component.Markup);
        foreach (var friend in friends)
        {
            Assert.True(await CompAddMember_CheckInviteListEntry(subjectUser, friend, subjectGroup, testInvites, component));
        }

        var target = subjectGroup.Users.First(u => u.Id != subjectUser.Id);
        await UserGroupService.RemoveGroupMember(subjectGroup.ID, target.Id);
        
        subjectGroup.Users.Remove(subjectGroup.Users.First(u => u.Id != subjectUser.Id));
        
    }

    [Fact]
    public async void GroupChatTests_CompChatSettings()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = await BuildTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();
        
        
    }

    [Fact]
    public async void GroupChatTests_CompChatView()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = await BuildTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();
    }
}