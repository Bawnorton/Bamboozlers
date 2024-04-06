using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Components.Group.Settings;
using Bamboozlers.Components.MainVisual;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Tests.Group;

public class GroupSettingsTests : AuthenticatedBlazoriseTestBase
{
    private (List<User>,List<Friendship>,List<GroupChat>,List<GroupInvite>) BuildTestCases()
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
        
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(groupChat1);
        MockDatabaseProvider.GetMockAppDbContext().MockChats.AddMock(groupChat2);

        var invite = new GroupInvite(testUsers[0].Id,testUsers[4].Id, groupChat1.ID)
        {
            Recipient = testUsers[4],
            Sender = testUsers[0],
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
    
    private void GroupSettingsTests_CheckMemberDisplay(
        User subjectUser, 
        GroupChat subjectGroup, 
        IRenderedFragment fragment)
    {
        foreach (var member in subjectGroup.Users)
        {
            var isMod = subjectGroup.Moderators.FirstOrDefault(u => u.Id == member.Id) is not null;
            var memberDiv = fragment.Find($"#{member.UserName}_section");
            if (isMod ||
                member.Id == subjectGroup.OwnerID)
            {
                var badge = fragment.Find($"#{member.UserName}_badge");
                Assert.Contains(
                    member.Id == subjectGroup.OwnerID 
                        ? "OWNER" 
                        : "MODERATOR", 
                    badge.TextContent
                );
            }

            if (subjectUser.Id == subjectGroup.OwnerID)
            {
                if (member.Id != subjectUser.Id)
                {
                    var kickButton = fragment.Find($"#{member.UserName}_kickButton");
                    var permsButton = fragment.Find($"#{member.UserName}_permsButton");
                    if (isMod)
                    {
                        Assert.Contains("Revoke permissions", permsButton.TextContent);
                    }
                    else
                    {
                        Assert.Contains("Assign permissions", permsButton.TextContent);
                    }
                    Assert.Contains("Kick user", kickButton.TextContent);
                }
            } 
            else if (!isMod)
            {
                var kickButton = fragment.Find($"#{member.UserName}_kickButton");
                Assert.Contains("Kick user", kickButton.InnerHtml);
            }
        }
    }
    
    [Fact]
    public async void GroupSettingsTests_CompChatSettings()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = BuildTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        var component = Ctx.RenderComponent<CompGroupSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );

        var modList = component.Find("#modList");
        var memberList = component.Find("#memberList");
        
        // Assert: Expected number of entries
        Assert.Equal(subjectGroup.Users.Count, memberList.Children.Length);
        
        // Assert: That expected elements are present
        GroupSettingsTests_CheckMemberDisplay(subjectUser,subjectGroup,component);
        
        // Assert: Check observed the observed Chats for component
        Assert.Equal(subjectGroup.ID, component.Instance.WatchedIDs[0]);
    }

    [Fact]
    public async void GroupSettingsTests_DeleteAvatarTest()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = BuildTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        var component = Ctx.RenderComponent<CompGroupSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        await component.Instance.DeleteAvatar();
        var alertArgs = component.Instance.AlertArguments;
        Assert.False(alertArgs.AlertVisible);

        subjectGroup.Avatar = new byte[1];
        component = Ctx.RenderComponent<CompGroupSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        await component.InvokeAsync(async () => await component.Instance.DeleteAvatar());
        alertArgs = component.Instance.AlertArguments;
        Assert.NotNull(alertArgs);
        Assert.Equal("Success!",alertArgs.AlertMessage);
        Assert.Equal("Group avatar was deleted.",alertArgs.AlertDescription);
    }

    [Fact]
    public async void GroupSettingsTests_ChangeAvatarTest()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = BuildTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();
        
        // Arrange, Act, Assert: Valid file upload
        var component = Ctx.RenderComponent<CompGroupSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        var res = false;
        await component.InvokeAsync(async () =>
        {
            res = await component.Instance.OnAvatarChange(new byte[1]);
        });
        Assert.True(res);
        res = false;
        
        // Arrange, Act, Assert: Chat is null
        component = Ctx.RenderComponent<CompGroupSettings>();
        await component.InvokeAsync(async () =>
        {
            res = await component.Instance.OnAvatarChange(new byte[1]);
        });
        Assert.False(res);
    }

    [Fact]
    public async void GroupSettingsTests_KickWarningPopupTest()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = BuildTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        AlertPopupArgs? returnedAlertPopupArgs = null;
        var fauxAlertPopupCallback = EventCallback.Factory.Create<AlertPopupArgs>(
            this,
            args =>
            {
                returnedAlertPopupArgs = args;
            } 
        );
        
        var component = Ctx.RenderComponent<CompGroupSettings>(
            parameters=>
            {
                parameters.Add(p => p.ChatID, subjectGroup.ID);
                parameters.Add(p => p.OpenAlertPopup, fauxAlertPopupCallback);
            });
        var actionButton = component.Find("#TestUser1_kickButton");
        await actionButton.ClickAsync(new MouseEventArgs());
        Assert.NotNull(returnedAlertPopupArgs);
        await component.InvokeAsync(() => returnedAlertPopupArgs.OnConfirmCallback.InvokeAsync());
    }
}