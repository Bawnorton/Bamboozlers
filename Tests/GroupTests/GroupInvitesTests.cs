using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Components.Group;
using Bunit.Extensions.WaitForHelpers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Tests.Provider;
using Xunit.Abstractions;

namespace Tests.GroupTests;

public class GroupInvitesTests : GroupChatTestBase
{
    private ITestOutputHelper output { get; set; }

    public GroupInvitesTests(ITestOutputHelper helper)
    {
        output = helper;
    }
    private async Task<bool> CompAddMember_CheckInviteListEntry(
        User self,
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
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();
        
        var component = Ctx.RenderComponent<CompAddMember>(
            parameters 
                => parameters.Add(p => p.ChatID, 1)
        );
        
        // Assert: Check observed the observed Chats for component
        Assert.Equal(subjectGroup.ID, component.Instance.WatchedIDs[0]);
        
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
                => parameters.Add(p => p.ChatID, 1)
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
    }

    [Fact]
    public async void GroupInvitesTests_SendInvite()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();
        
        var component = Ctx.RenderComponent<CompAddMember>(
            parameters 
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );

        var friend = testUsers[3];
        var actionButton = component.WaitForElement($"#{friend.UserName}-button");
        Assert.Contains("Invite",actionButton.TextContent);

        await actionButton.ClickAsync(new MouseEventArgs());

        var dbContext = await MockDatabaseProvider.GetDbContextFactory().CreateDbContextAsync();
        Assert.NotNull(MockDatabaseProvider.GetMockAppDbContext().MockGroupInvites.MockDbSet.Object.FirstOrDefault(i =>
            i.GroupID == subjectGroup.ID && i.SenderID == subjectUser.Id && i.RecipientID == friend.Id));
        Assert.NotNull(dbContext.GroupInvites.FirstOrDefault(i =>
            i.GroupID == subjectGroup.ID && i.SenderID == subjectUser.Id && i.RecipientID == friend.Id));
        
        actionButton = component.WaitForElement($"#{friend.UserName}-button");
        Assert.Contains("Pending",actionButton.TextContent);
    }
    
    [Fact]
    public async void GroupInvitesTests_CompGroupInvites()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        testInvites.AddRange(new GroupInvite[]
        {
            new(0, 5, 1)
            {
                Group = subjectGroup,
                Sender = subjectUser,
                Recipient = testUsers[5]
            },
            new(5, 0, 2)
            {
                Group = testGroups[1],
                Sender = testUsers[5],
                Recipient = subjectUser
            },
            new(6, 0, 2)
            {
                Group = testGroups[1],
                Sender = testUsers[5],
                Recipient = subjectUser
            },
            new(4, 0, 3)
            {
                Group = testGroups[2],
                Sender = testUsers[4],
                Recipient = subjectUser
            },
            new(0, 1, 3)
            {
                Group = testGroups[2],
                Sender = testUsers[4],
                Recipient = subjectUser
            }
        });

        List<GroupInvite> incoming = [];
        List<GroupInvite> outgoing = [];
        foreach (var inv in testInvites)
        {
            MockDatabaseProvider.GetMockAppDbContext().MockGroupInvites.AddMock(inv);
        }

        foreach (var inv in MockDatabaseProvider.GetMockAppDbContext().MockGroupInvites.GetMocks())
        {
            if (inv.Sender == subjectUser)
            {
                outgoing.Add(inv);
            } 
            else if (inv.Recipient == subjectUser)
            {
                incoming.Add(inv);
            }
        }
        
        var component = Ctx.RenderComponent<CompGroupInvites>();
        var incomingBadge = component.Find("#incoming-badge");
        Assert.Contains($"{incoming.Count}", incomingBadge.TextContent);
        
        var outgoingBadge = component.Find("#outgoing-badge");
        Assert.Contains($"{outgoing.Count}", outgoingBadge.TextContent);

        var incomingInvites = component.FindAll("#group-invite-incoming");
        Assert.Equal(incoming.Count,incomingInvites.Count);
        
        for (var i = 0; i < incoming.Count; i++)
        {
            var inviteDiv = incomingInvites[i];
            var invite = incoming[i];
            
            var firstPart = inviteDiv.Children[0];
            var secondPart = inviteDiv.Children[1];
            var accept = inviteDiv.Children[2].FirstElementChild!;
            var decline = inviteDiv.Children[3].FirstElementChild!;
            
            Assert.Equal($"forgroup-{invite.Group.GetGroupName()}",firstPart.Id);
            Assert.Contains("Invite to", firstPart.TextContent);
            Assert.Contains(invite.Group.GetGroupName(), firstPart.TextContent);

            if (invite.Group.Avatar is not null)
            {
                var avatarDisplayed = firstPart.FindChild<IHtmlImageElement>();
                Assert.NotNull(avatarDisplayed);
            }
            
            Assert.Equal($"fromuser-{invite.Sender.UserName}", secondPart.Id);
            Assert.Contains("from", secondPart.TextContent);
            
            Assert.Equal("accept-button",accept.Id);
            Assert.Equal("decline-button",decline.Id);
        }
        
        component.Find("#outgoing-toggle").FirstElementChild!.Click();
        var outgoingInvites = component.FindAll("#group-invite-outgoing");
        Assert.Equal(outgoing.Count,outgoingInvites.Count);
        
        for (var i = 0; i < outgoing.Count; i++)
        {
            var inviteDiv = outgoingInvites[i];
            var invite = outgoing[i];
            
            var firstPart = inviteDiv.Children[0];
            var secondPart = inviteDiv.Children[1];
            var button = inviteDiv.Children[2].FirstElementChild!;
            
            Assert.Equal($"forgroup-{invite.Group.GetGroupName()}",firstPart.Id);
            Assert.Contains("Invite to", firstPart.TextContent);
            Assert.Contains(invite.Group.GetGroupName(), firstPart.TextContent);

            if (invite.Group.Avatar is not null)
            {
                var avatarDisplayed = firstPart.FindChild<IHtmlImageElement>();
                Assert.NotNull(avatarDisplayed);
            }
            
            Assert.Equal($"touser-{invite.Sender.UserName}", secondPart.Id);
            Assert.Contains("for", secondPart.TextContent);
            
            Assert.Equal("accept-button",button.Id);
        }
    }
}