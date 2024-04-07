using Bamboozlers.Classes.Data;
using Bamboozlers.Components.Group;
using Microsoft.AspNetCore.Components;

namespace Tests.GroupTests;

public class GroupCreateTests : GroupChatTestBase
{
    [Fact]
    public async void GroupCreateTests_CreateGroup()
    {
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);
        
        var calledToClose = false;
        OpenChatArgs? returnedOpenChatArgs = null;
        var fauxClosePopupCallback = EventCallback.Factory.Create(this, () =>
        {
            calledToClose = true;
        });
        var fauxOpenChatCallback = EventCallback.Factory.Create<OpenChatArgs>(this, args =>
        {
            returnedOpenChatArgs = args;
        });

        var component = Ctx.RenderComponent<CompCreateGroup>(parameters =>
        {
            parameters.Add(p => p.ClosePopupCallback, fauxClosePopupCallback);
            parameters.Add(p => p.OpenChatCallback, fauxOpenChatCallback);
        });

        await component.Instance.CreateGroup();
        Assert.True(calledToClose);
        Assert.NotNull(returnedOpenChatArgs);
        Assert.Equal(ChatType.Group, returnedOpenChatArgs.ChatType);
        Assert.Equal(0, returnedOpenChatArgs.Id);
        
        // Arrange: User is null, so operation will fail
        calledToClose = false;
        returnedOpenChatArgs = null;
        
        await SetUser(null);
        UserService.Invalidate();
        
        await component.Instance.CreateGroup();
        Assert.False(calledToClose);
        Assert.Null(returnedOpenChatArgs);
        var alertArgs = component.Instance.AlertArguments;
        Assert.Equal("Could not create group at this time.", alertArgs.AlertMessage);
        Assert.Equal("An error occurred preventing the creation of the group.", alertArgs.AlertDescription);
        
        component.Dispose();
    }

    [Fact]
    public async void GroupCreateTests_UploadAndDeleteAvatar()
    {
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);
        
        var component = Ctx.RenderComponent<CompCreateGroup>();

        var res = await component.Instance.UploadAvatar(new byte[1]);
        Assert.True(res);
        Assert.Equal(new byte[1], component.Instance.Avatar);

        component.Instance.DeleteAvatar();
        Assert.Null(component.Instance.Avatar);

        res = await component.Instance.UploadAvatar(null);
        Assert.False(res);

        res = await component.Instance.UploadAvatar(Array.Empty<byte>());
        Assert.False(res);
    }
}