using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Blazorise.Modules;
using Bunit.Extensions.WaitForHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
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
        var (testUsers, testFriendships, testGroups, testInvites) 
            = await BuildTestCases();
        
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

    private void CompChatSettings_CheckMemberDisplay(
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
    public async void GroupChatTests_CompChatSettings()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = await BuildTestCases();
        
        // Arrange: User is Owner
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();

        var component = Ctx.RenderComponent<CompChatSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );

        var modList = component.Find("#modList");
        var memberList = component.Find("#memberList");
        
        output.WriteLine(component.Markup);
        // Assert: Expected number of entries
        Assert.Equal(subjectGroup.Users.Count, memberList.Children.Length);
        
        // Assert: That expected elements are present
        CompChatSettings_CheckMemberDisplay(subjectUser,subjectGroup,component);
        
        // Assert: Check observed the observed Chats for component
        Assert.Equal(subjectGroup.ID, component.Instance.WatchedIDs[0]);
        
        // Arrange: User is Moderator
        subjectUser = testUsers[1];
        await SetUser(subjectUser);
        UserService.Invalidate();

        component.Dispose();
        component = Ctx.RenderComponent<CompChatSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        // Arrange: User is Member
        subjectUser = testUsers[2];
        await SetUser(subjectUser);
        UserService.Invalidate();

        component.Dispose();
        component = Ctx.RenderComponent<CompChatSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
    }

    [Fact]
    public async void CompChatSettings_ChangeAvatarTest()
    {
        // Arrange & Act: Set up test cases for Users and Group Chats
        var (testUsers, testFriendships, testGroups, testInvites) 
            = await BuildTestCases();
        
        // Arrange: User is Owner
        var subjectUser = testUsers[0];
        var subjectGroup = testGroups[0];
        await SetUser(subjectUser);
        UserService.Invalidate();
        
        var component = Ctx.RenderComponent<CompChatSettings>(
            parameters
                => parameters.Add(p => p.ChatID, subjectGroup.ID)
        );
        
        // Arrange: No file passed
        var spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile>());
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        var alertArgs = component.Instance.AlertArguments;
        Assert.True(alertArgs.AlertVisible);
        Assert.Equal("Error occured while uploading image.", alertArgs.AlertMessage);
        Assert.Equal("No file was uploaded.",alertArgs.AlertDescription);
        
        // Arrange: Invalid file passed (not an image)
        var fakeFile = new MockBrowserFile { ContentType = "file/csv" };
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        alertArgs = component.Instance.AlertArguments;
        Assert.Equal("Uploaded file was not an image.",alertArgs.AlertDescription);
        
        // Arrange: Invalid file passed (image, but not png)
        fakeFile = new MockBrowserFile { ContentType = "image/gif" };
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        alertArgs = component.Instance.AlertArguments;
        Assert.Equal("Image must be a PNG or JPG (JPEG) file.",alertArgs.AlertDescription);
        
        // Arrange: Valid file passed, but image was empty
        fakeFile = new MockBrowserFile { ContentType = "image/png", Bytes = Array.Empty<byte>()};
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        alertArgs = component.Instance.AlertArguments;
        Assert.Equal("Unknown error occurred. Please try again.",alertArgs.AlertDescription);
        
        // Arrange: Valid file passed
        fakeFile = new MockBrowserFile { ContentType = "image/png"};
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        alertArgs = component.Instance.AlertArguments;
        Assert.Equal("Image was successfully uploaded.",alertArgs.AlertDescription);
        
        await component.Find("#settings-save").ClickAsync(new MouseEventArgs());
        // TODO: This is really bad, but we can't exactly mock SQL calls in an easy way. Thus, the result is not checked.
    }
}