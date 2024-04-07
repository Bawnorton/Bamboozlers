using AngleSharp.Dom;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Utility.Observer;
using Bamboozlers.Components.Group;
using Bunit.Extensions.WaitForHelpers;
using Xunit.Abstractions;

namespace Tests.GroupTests;

public class GroupInvitesTests(ITestOutputHelper helper) : GroupChatTestBase
{
    private ITestOutputHelper Output { get; set; } = helper;

    private Task<bool> CompAddMember_CheckInviteListEntry(
        User self,
        User friend, 
        Chat chat,
        IEnumerable<GroupInvite> invites,
        IRenderedFragment fragment)
    {
        try
        {
            fragment.WaitForElement($"#user-{friend.UserName}");
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
                Assert.Contains("Revoke Invite",actionButton.TextContent);
            }
            else
            {
                Assert.Contains("Invite",actionButton.TextContent);
            }
        }
        catch (WaitForFailedException e)
        {
            Output.WriteLine(e.Message);
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
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
        
        // Observer Pattern Test
        var subjectFriendship = testFriendships[0];
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.RemoveMock(subjectFriendship);
        await component.Instance.OnUpdate(GroupEvent.General);
        
        Assert.Throws<WaitForFailedException>(() => component.WaitForElement($"#user-{subjectFriendship.User2.UserName}"));
        
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
    public async void GroupInvitesTests_CompGroupInvites()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, _, _, testInvites) = BuildGroupTestCases();
        
        var subjectUser = testUsers[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        List<GroupInvite> incoming = [];
        List<GroupInvite> outgoing = [];

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
        
        foreach (var invite in incoming)
        {
            var inviteDiv = component.Find($"#incoming-invites #user-{invite.Sender.UserName}");
            
            var username = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "username");
            Assert.NotNull(username);
            Assert.Contains($"{invite.Sender.UserName}",username.TextContent);
            
            var innerContent = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "inner-content-div");
            Assert.NotNull(innerContent);
            var innerContentText = innerContent.Descendants<IText>().FirstOrDefault();
            Assert.NotNull(innerContentText);
            Assert.Contains("(Invite for group",innerContentText.TextContent);
            Assert.Contains($"{invite.Group.GetGroupName()})",innerContentText.TextContent);
            
            var acceptButton = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "accept-button");
            Assert.NotNull(acceptButton);
            
            var declineButton = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "decline-button");
            Assert.NotNull(declineButton);
        }
        
        component.Find("#outgoing-toggle").FirstElementChild!.Click();
        foreach (var invite in outgoing)
        {
            var inviteDiv = component.Find($"#outgoing-invites #user-{invite.Recipient.UserName}");
            
            var username = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "username");
            Assert.NotNull(username);
            Assert.Contains($"{invite.Recipient.UserName}",username.TextContent);
            
            var innerContent = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "inner-content-div");
            Assert.NotNull(innerContent);
            var innerContentText = innerContent.Descendants<IText>().FirstOrDefault();
            Assert.NotNull(innerContentText);
            Assert.Contains("(Invite for group",innerContentText.TextContent);
            Assert.Contains($"{invite.Group.GetGroupName()})",innerContentText.TextContent);
            
            var revokeButton = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "revoke-button");
            Assert.NotNull(revokeButton);
        }
        
        // Observer Pattern
        component.Find("#incoming-toggle").FirstElementChild!.Click();
        var subjectInvite = testInvites[0];
        MockDatabaseProvider.GetMockAppDbContext().MockGroupInvites.RemoveMock(subjectInvite);
        await component.Instance.OnUpdate(GroupEvent.General);
        
        Assert.Throws<WaitForFailedException>(() => component.WaitForElement($"#incoming-invites #user-{subjectInvite.Sender.UserName}"));
    }
}