using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Components.Group;
using Bunit.Extensions.WaitForHelpers;

namespace Tests.Group;

public class GroupInvitesTests : GroupChatTestBase
{
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
}