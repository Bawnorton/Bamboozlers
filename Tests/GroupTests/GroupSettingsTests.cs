using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Components.Group.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Tests.GroupTests;

public class GroupSettingsTests : GroupChatTestBase
{
    private void GroupSettingsTests_CheckMemberDisplay(User subjectUser, User member, GroupChat subjectGroup, IRenderedFragment fragment)
    {
        var isMod = subjectGroup.Moderators.FirstOrDefault(u => u.Id == member.Id) is not null;
        var subjectIsMod = subjectGroup.Moderators.FirstOrDefault(u => u.Id == subjectUser.Id) is not null;
        var memberDiv = fragment.Find($"#{member.UserName}_section");
        if (isMod || member.Id == subjectGroup.OwnerID)
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
        else if (subjectIsMod)
        {
            if (!isMod && member.Id != subjectGroup.OwnerID)
            {
                var kickButton = fragment.Find($"#{member.UserName}_kickButton");
                Assert.Contains("Kick user", kickButton.InnerHtml);   
            }
        }
    }
    
    private void GroupSettingsTests_CheckModList(
        User subjectUser, 
        GroupChat subjectGroup, 
        IRenderedFragment fragment)
    {
        var list = new List<User> {subjectGroup.Owner};
        list.AddRange(subjectGroup.Moderators);
        foreach (var member in list)
        {
            GroupSettingsTests_CheckMemberDisplay(subjectUser,member,subjectGroup,fragment);
        }
    }
    
    private void GroupSettingsTests_CheckMemberList(
        User subjectUser, 
        GroupChat subjectGroup, 
        IRenderedFragment fragment)
    {
        foreach (var member in subjectGroup.Users)
        {
            GroupSettingsTests_CheckMemberDisplay(subjectUser,member,subjectGroup,fragment);
        }
    }
    
    [Fact]
    public async void GroupSettingsTests_CompChatSettings()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
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
        Assert.Equal(subjectGroup.Moderators.Count+1, modList.Children.Length);
        Assert.Equal(subjectGroup.Users.Count, memberList.Children.Length);
        
        // Assert: That expected elements are present
        GroupSettingsTests_CheckMemberList(subjectUser,subjectGroup,component);
        GroupSettingsTests_CheckModList(subjectUser, subjectGroup, component);
        
        // Assert: Check observed the observed Chats for component
        Assert.Equal(subjectGroup.ID, component.Instance.WatchedIDs[0]);
        
        // Arrange: User is a Moderator
        subjectUser = testUsers[2];
        subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        component = Ctx.RenderComponent<CompGroupSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        modList = component.Find("#modList"); 
        memberList = component.Find("#memberList");
        
        // Assert: Expected number of entries
        Assert.Equal(subjectGroup.Moderators.Count+1, modList.Children.Length);
        Assert.Equal(subjectGroup.Users.Count, memberList.Children.Length);
        
        // Assert: That expected elements are present
        GroupSettingsTests_CheckMemberList(subjectUser,subjectGroup,component);
        GroupSettingsTests_CheckModList(subjectUser, subjectGroup, component);
        
        // Assert: Check observed the observed Chats for component
        Assert.Equal(subjectGroup.ID, component.Instance.WatchedIDs[0]);
        
        // Arrange: User is a Member
        subjectUser = testUsers[1];
        subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        component = Ctx.RenderComponent<CompGroupSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        modList = component.Find("#modList"); 
        memberList = component.Find("#memberList");
        
        // Assert: Expected number of entries
        Assert.Equal(subjectGroup.Moderators.Count+1, modList.Children.Length);
        Assert.Equal(subjectGroup.Users.Count, memberList.Children.Length);
        
        // Assert: That expected elements are present
        GroupSettingsTests_CheckMemberList(subjectUser,subjectGroup,component);
        GroupSettingsTests_CheckModList(subjectUser, subjectGroup, component);
        
        // Assert: Check observed the observed Chats for component
        Assert.Equal(subjectGroup.ID, component.Instance.WatchedIDs[0]);
    }

    [Fact]
    public async void GroupSettingsTests_DeleteAvatarTest()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
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
        
        subjectGroup.Avatar = new byte[1];
        component = Ctx.RenderComponent<CompGroupSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        await SetUser(null);
        UserService.Invalidate();
        await component.Instance.DeleteAvatar();
        alertArgs = component.Instance.AlertArguments;
        Assert.NotNull(alertArgs);
        Assert.Equal("Unsuccessful attempt to delete avatar.",alertArgs.AlertMessage);
        Assert.Equal("Issue occurred that prevented group changes from being saved.",alertArgs.AlertDescription);
    }

    [Fact]
    public async void GroupSettingsTests_ChangeAvatarTest()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
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
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
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

    [Fact]
    public async void GroupSettingsTests_CompEditGroupName()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) = BuildGroupTestCases();
        
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        AlertArguments? returnedAlertArguments = null;
        EventCallback<AlertArguments> fauxAlertCallback = EventCallback.Factory.Create<AlertArguments>(
            this,
            args =>
            {
                returnedAlertArguments = args;
            }    
        );
        var component = Ctx.RenderComponent<CompEditGroupName>(
            parameters=>
            {
                parameters.Add(p => p.ChatID, subjectGroup.ID);
                parameters.Add(p => p.AlertCallback, fauxAlertCallback);
            });
        await component.Instance.OnSave();
        Assert.NotNull(returnedAlertArguments);
        Assert.Equal("Cannot change group name.", returnedAlertArguments.AlertMessage);
        Assert.Equal("Specified new name is the same as the previous name.", returnedAlertArguments.AlertDescription);
        returnedAlertArguments = null;

        component.Instance.TypedName = "New Name";
        await component.Instance.OnSave();
        Assert.NotNull(returnedAlertArguments);
        Assert.Equal("Success!", returnedAlertArguments.AlertMessage);
        Assert.Equal("Group name was changed.", returnedAlertArguments.AlertDescription);
        returnedAlertArguments = null;

        await SetUser(null);
        UserService.Invalidate();
        component.Instance.TypedName = "New Name 2";
        await component.Instance.OnSave();
        Assert.NotNull(returnedAlertArguments);
        Assert.Equal("Unsuccessful attempt to save changes.", returnedAlertArguments.AlertMessage);
        Assert.Equal("Issue occurred that prevented group changes from being saved.", returnedAlertArguments.AlertDescription);
        returnedAlertArguments = null;
        
        component = Ctx.RenderComponent<CompEditGroupName>(
            parameters=>
            {
                parameters.Add(p => p.ChatID, -1);
                parameters.Add(p => p.AlertCallback, fauxAlertCallback);
            });
        await component.Instance.OnSave();
        Assert.NotNull(returnedAlertArguments);
        Assert.Equal("An error occurred while saving changes.", returnedAlertArguments.AlertMessage);
        Assert.Equal("An unknown error occurred. Please try again.", returnedAlertArguments.AlertDescription);
    }
}