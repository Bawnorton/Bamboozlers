using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Classes.Utility.Observer;
using Bamboozlers.Components;
using Bamboozlers.Components.Utility;
using Blazorise;
using Bunit.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Tests.UserTests;

namespace Tests.CoreTests;

public class MiscTests : AuthenticatedBlazoriseTestBase
{
    [Fact]
    public async void MiscTests_AvatarUploader()
    {
        var resultArgs = AlertArguments.DefaultErrorAlertArgs;
        var deleteSignalled = false;
        var component = Ctx.RenderComponent<CompAvatarUploader>(
            parameters
                =>
            {
                parameters.Add(p => p.OnDelete, () => deleteSignalled = true);
                parameters.Add(p => p.AlertCallback, (args) => resultArgs = args);
                parameters.Add(p => p.OnUpload, bytes => Task.FromResult(!bytes.IsNullOrEmpty()));
            }
        );
        
        // Arrange: No file passed
        var spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile>());
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.True(resultArgs.AlertVisible);
        Assert.Equal("Error occured while uploading image.", resultArgs.AlertMessage);
        Assert.Equal("No file was uploaded.", resultArgs.AlertDescription);
        
        // Arrange: Invalid file passed (not an image)
        var fakeFile = new MockBrowserFile { ContentType = "file/csv" };
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Uploaded file was not an image.", resultArgs.AlertDescription);
        
        // Arrange: Invalid file passed (image, but not png)
        fakeFile = new MockBrowserFile { ContentType = "image/gif" };
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Image must be a PNG or JPG (JPEG) file.", resultArgs.AlertDescription);
        
        // Arrange: Valid file passed, but image was empty
        fakeFile = new MockBrowserFile { ContentType = "image/png", Bytes = Array.Empty<byte>()};
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Unknown error occurred. Please try again.", resultArgs.AlertDescription);
        
        // Arrange: Valid file passed
        fakeFile = new MockBrowserFile { ContentType = "image/png"};
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act
        await component.Instance.OnFileUpload(spoofArgs);
        // Assert
        Assert.Equal("Image was successfully uploaded.", resultArgs.AlertDescription);

        // Arrange: Valid file passed, but an (induced) Exception occured
        component = Ctx.RenderComponent<CompAvatarUploader>(
            parameters
                =>
            {
                parameters.Add(p => p.OnDelete, () => deleteSignalled = true);
                parameters.Add(p => p.AlertCallback, (args) => resultArgs = args);
                parameters.Add(p => p.OnUpload, bytes => Task.FromResult(false));
            }
        );
        fakeFile = new MockBrowserFile { ContentType = "image/png"};
        spoofArgs = new InputFileChangeEventArgs(new List<IBrowserFile> { fakeFile });
        // Act & Assert
        await component.Instance.OnFileUpload(spoofArgs);
        Assert.Equal("Unknown error occurred during upload. Please try again.", resultArgs.AlertDescription);
        
        // Arrange: Unsuccessful Signal Delete Avatar
        component = Ctx.RenderComponent<CompAvatarUploader>(
            parameters
                =>
            {
                parameters.Add(p => p.OnDelete, () => deleteSignalled = true);
                parameters.Add(p => p.AlertCallback, (args) => resultArgs = args);
                parameters.Add(p => p.OnUpload, bytes => Task.FromResult(!bytes.IsNullOrEmpty()));
            }
        );
        // Act
        await component.Instance.SignalDeleteAvatar();
        // Assert
        Assert.False(deleteSignalled);
        
        // Arrange: Successful Signal Delete Avatar
        component = Ctx.RenderComponent<CompAvatarUploader>(
            parameters
                =>
            {
                parameters.Add(p => p.OnDelete, () => deleteSignalled = true);
                parameters.Add(p => p.AlertCallback, (args) => resultArgs = args);
                parameters.Add(p => p.OnUpload, bytes => Task.FromResult(!bytes.IsNullOrEmpty()));
                parameters.Add(p => p.Avatar, Array.Empty<byte>());
            }
        );
        // Act
        await component.Instance.SignalDeleteAvatar();
        // Assert
        Assert.True(deleteSignalled);
        
        
        component.Dispose();
    }

    [Theory]
    [InlineData(GroupEvent.SentInvite, IconName.UserPlus, "Send Invite")]
    [InlineData(GroupEvent.SentInviteRevoked, IconName.Ban, "Revoke Invite")]
    [InlineData(GroupEvent.SentInvitePending, IconName.Send, "Pending")]
    [InlineData(GroupEvent.ReceivedInviteAccepted, IconName.Check, "")]
    [InlineData(GroupEvent.ReceivedInviteDeclined, IconName.Ban, "")]
    [InlineData(GroupEvent.RemoveMember, IconName.Ban, "Kick User")]
    
    public async void MiscTests_CompGroupButton(
        GroupEvent eventType,
        IconName expectedIconName,
        string? expectedCaption
    )
    {
        await SetUser(MockUserManager.CreateMockUser(0));
        var component = Ctx.RenderComponent<CompGroupButton>(parameters =>
        {
            parameters.Add(p => p.EventKind, eventType);
        });
        
        Assert.Equal(expectedIconName,component.Instance.IconName);
        Assert.Equal(expectedCaption, component.Instance.ButtonCaption);

        if (eventType is GroupEvent.SentInvitePending)
            Assert.True(component.Instance.Disabled);
        
        Assert.True(component.Instance.GroupCallback.HasDelegate);
        var count = 0;

        await component.InvokeAsync(() => component.Instance.GroupCallback.InvokeAsync());
        var completionCall = new TaskCompletionSource<bool>();
        component.Instance.GroupCallback = EventCallback.Factory.Create(this,
            () =>
            {
                completionCall.SetResult(true);
                ++count;
            });
        await component.Instance.GroupCallback.InvokeAsync();
        await completionCall.Task.WaitAsync(CancellationToken.None);
        Assert.Equal(1,count);
        
        component.Dispose();
    }

    [Theory]
    [InlineData(InteractionEvent.Unblock, IconName.Shield, "Unblock User")]
    [InlineData(InteractionEvent.RequestRevoked,IconName.Ban,"Revoke Friend Request")]
    [InlineData(InteractionEvent.RequestDeclined,IconName.Ban,"")]
    [InlineData(InteractionEvent.RequestAccepted,IconName.Check,"")]
    [InlineData(InteractionEvent.RequestSent,IconName.UserPlus,"Send Friend Request")]
    [InlineData(InteractionEvent.RequestPending,IconName.Send,"Pending")]
    public async void MiscTests_CompInteractionButton(
        InteractionEvent eventType,
        IconName expectedIconName,
        string? expectedCaption
        )
    {
        await SetUser(MockUserManager.CreateMockUser(0));
        var component = Ctx.RenderComponent<CompInteractionButton>(parameters =>
        {
            parameters.Add(p => p.EventKind, eventType);
        });
        
        Assert.Equal(expectedIconName,component.Instance.IconName);
        Assert.Equal(expectedCaption, component.Instance.ButtonCaption);
        
        if (eventType is InteractionEvent.RequestPending)
            Assert.True(component.Instance.Disabled);
        
        Assert.True(component.Instance.InteractionCallback.HasDelegate);
        var count = 0;
        await component.InvokeAsync(() => component.Instance.InteractionCallback.InvokeAsync());
        var completionCall = new TaskCompletionSource<bool>();
        component.Instance.InteractionCallback = EventCallback.Factory.Create(this,
            () =>
            {
                completionCall.SetResult(true);
                ++count;
            });
        await component.Instance.InteractionCallback.InvokeAsync();
        await completionCall.Task.WaitAsync(CancellationToken.None);
        Assert.Equal(1,count);
        
        component.Dispose();
    }

    [Fact]
    public async void MiscTests_CompInteractUsername()
    {
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        var completionCall = new TaskCompletionSource<bool>();
        KnownPopupArgs popupArgs = new KnownPopupArgs(PopupType.Settings);
        EventCallback<KnownPopupArgs> fauxPopupCallback = EventCallback.Factory.Create<KnownPopupArgs>(
            this,
            args =>
            {
                popupArgs = args;
                completionCall.SetResult(true);
            }
        );
        
        var component = Ctx.RenderComponent<CompInteractUsername>(
            parameters =>
            {
                parameters.Add(p => p.OpenKnownPopup, fauxPopupCallback);
                parameters.Add(p => p.User, user);
            }
        );

        var element = component.Find("#interact-username");
        Assert.Contains(user.UserName!, element.TextContent);

        await element.ClickAsync(new MouseEventArgs());
        await completionCall.Task.WaitAsync(CancellationToken.None);
        Assert.Equal(PopupType.UserProfile, popupArgs.Type);
        Assert.Equal(user, popupArgs.FocusUser);

        component.Dispose();
    }

    [Fact]
    public async void MiscTests_UserViewComponentBase_Subscriptions()
    {
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);
        var component = Ctx.RenderComponent<UserViewComponentBase>();

        // Very trivial, just want to make sure the lines are run
        await component.Instance.OnUpdate();
        ((ISubscriber) component.Instance).OnUpdate();
        
        var data = component.Instance.UserData;
        Assert.NotNull(data);
        Assert.Equal(user.UserName, data.UserName);
    }
}