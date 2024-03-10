using System.ComponentModel.DataAnnotations;
using AngleSharp.Dom;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Components.Settings;
using Bamboozlers.Components.Settings.EditFields;
using Bamboozlers.Layout;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Identity;
using Blazorise.Tests.bUnit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Tests;

public class UserSettingsTests : BlazoriseTestBase
{
    private readonly MockAuthenticationProvider _mockAuthenticationProvider;
    private readonly MockUserManager _mockUserManager;
    private readonly ITestOutputHelper output;
    public UserSettingsTests(ITestOutputHelper output)
    {
        this.output = output;
        // Setup UserManager, Mock AuthProvider (Helper)
        _mockUserManager = new MockUserManager(Ctx,output);
        _mockAuthenticationProvider = new MockAuthenticationProvider(Ctx, _mockUserManager.GetMockUser(0));
        AuthHelper.Init(_mockAuthenticationProvider.GetAuthStateProvider(), _mockUserManager.GetDbContextFactory());
        
        // Add Blazorise Necessities
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());
        
        // Set-Up JavaScript Interop
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        
        _mockUserManager.ClearMockUsers();
    }
    
    [Fact]
    public async Task UserSettingsTests_CompSettings()
    {
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);
        
        var component = Ctx.RenderComponent<CompSettings>();
        
        // Assert: By default, settings popup is invisible
        Assert.False(component.Instance.Visible);

        // Arrange: Set settings component to be visible via render parameter
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
        );
        Assert.True(component.Instance.Visible);
        
        // Assert: By default, status arguments should be no-parameter constructor result
        Assert.Equal(Color.Default,component.Instance.Arguments.StatusColor);
        Assert.False(component.Instance.Arguments.StatusVisible);
        Assert.Equal("",component.Instance.Arguments.StatusMessage);
        Assert.Equal("",component.Instance.Arguments.StatusDescription);
        Assert.Equivalent(new StatusArguments() {}, component.Instance.Arguments);

        // Assert: CompSettings does not have circular callback assigned
        var callback = component.Instance.StatusChangeEvent;
        Assert.NotEqual(EventCallback.Factory.Create<StatusArguments>(component.Instance,component.Instance.OnStatusUpdate), callback);
        
        // Act: Invoke Status Change
        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnStatusUpdate(new StatusArguments(
                Color.Light,
                true,
                "Test",
                "This is a test."
            ));
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert: Check that these are the values that are set
        Assert.Equal(Color.Light,component.Instance.Arguments.StatusColor);
        Assert.True(component.Instance.Arguments.StatusVisible);
        Assert.Equal("Test",component.Instance.Arguments.StatusMessage);
        Assert.Equal("This is a test.",component.Instance.Arguments.StatusDescription);
        
        _mockUserManager.ClearMockUsers();
        component.Dispose();
    }

    /// <summary>
    /// Tests the functionality of UserDisplayRecord in tandem with CompSettings.
    /// </summary>
    [Fact]
    public async Task UserSettingsTest_DisplayUser()
    {
        // Arrange: Set the user with some non-default variables
        var user = _mockUserManager.CreateMockUser(0,
            true,
            "Bobby",
            "Hi! I'm Bobby."
        );
        user.Email = "bobby.blazor@gmail.com";
        _mockAuthenticationProvider.SetUser(user);
        
        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
        );
        AuthHelper.Init(_mockAuthenticationProvider.GetAuthStateProvider(), _mockUserManager.GetDbContextFactory());
        
        // Act: Invoke Data Load (into UserDisplayRecord)
        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.LoadValuesFromStorage();
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);

        // Assert: Check if assigned values are as expected
        Assert.Equal("TestUser0",UserDisplayRecord.UserName);
        Assert.Equal("Bobby",UserDisplayRecord.DisplayName);
        Assert.Equal("bobby.blazor@gmail.com",UserDisplayRecord.Email);
        Assert.Equal("Hi! I'm Bobby.",UserDisplayRecord.Bio);
        Assert.Equal(UserDisplayRecord.GetDisplayableAvatar(user.Avatar), UserDisplayRecord.Avatar);
        
        // Arrange & Act: Change the data present
        
        completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord()
            {
                DisplayName = "Robert",
                Bio = "Hello, my name is Robert.",
                DataType = UserDataType.Visual
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);

        // Assert
        Assert.Equal("TestUser0",UserDisplayRecord.UserName);
        Assert.Equal("Robert",UserDisplayRecord.DisplayName);
        Assert.Equal("bobby.blazor@gmail.com",UserDisplayRecord.Email);
        Assert.Equal("Hello, my name is Robert.",UserDisplayRecord.Bio);
        
        // Arrange & Act: Destroy Bobby "Robert" Blazor for the purposes of testing
        _mockUserManager.ClearMockUsers();
        completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord { DataType = UserDataType.Visual });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert: That since his information is nullified, nothing should change.
        Assert.Equal("TestUser0",UserDisplayRecord.UserName);
        Assert.Equal("Robert",UserDisplayRecord.DisplayName);
        Assert.Equal("bobby.blazor@gmail.com",UserDisplayRecord.Email);
        Assert.Equal("Hello, my name is Robert.",UserDisplayRecord.Bio);
    }

    /// <summary>
    /// Tests the specific functionality of the ChangeUsername() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_ChangeUsername()
    {
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);
        
        UserUpdateResult? result = null;
        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        
        // Arrange: Username was set to be changed, but the new username wasn't sent in.
        var completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Username,
                CurrentPassword = user.PasswordHash
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Invalid Or Empty",result.Reason);
        
        // Arrange: Username was set to be changed, but the new username is the same as the old.
        completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Username,
                UserName = user.UserName,
                CurrentPassword = user.PasswordHash
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Same Username",result.Reason);
        
        // Arrange: User is not found
        _mockUserManager.ClearMockUsers();
        completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Username
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found",result.Reason);
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the specific functionality of the ChangePassword() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_ChangePassword()
    {
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);
        
        UserUpdateResult? result = null;
        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        
        // Arrange: Password was changed, but no values were sent in.
        var completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Password
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Password,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Missing Input",result.Reason);
        
        // Arrange: User is not found
        _mockUserManager.ClearMockUsers();
        completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Password
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Password,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found",result.Reason);
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the specific functionality of the ChangeEmail() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_ChangeEmail()
    {
        UserUpdateResult? result = null;
        
        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        
        // Arrange: User is not found
        _mockUserManager.ClearMockUsers();
        var completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Email
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Email,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found",result.Reason);
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the specific functionality of the ChangeUsername() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_DeleteAccount()
    {
        UserUpdateResult? result = null;
        
        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        
        // Arrange: User is not found
        _mockUserManager.ClearMockUsers();
        var completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Deletion
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Deletion,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found",result.Reason);
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditUsername component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditUsername()
    {
        UserSettingsTests_TabToggle<CompEditUsername>();
        await UserSettingsTests_NoDataChangeFunction<CompEditUsername>();
        
        var user0 = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user0);
        _mockUserManager.GetMockUser(1);

        UserUpdateResult? result = null;
        
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        var component = parentComponent.FindComponent<CompEditUsername>();
        
        var nameField = component.Find("#username-field");
        var passField = component.Find("#password-field");
        var submit = component.Find("#submit-button");
        
        // Arrange: Successful Change (Username is not taken)
        nameField.Change("TestUser2");
        passField.Change("@Password0");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.True(result.Success);
        Assert.Equal("",result.Reason);
        
        // Arrange: Unsuccessful Change (Username is taken)
        nameField.Change("TestUser1");
        passField.Change("@Password0");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Error Occurred",result.Reason);
        
        // Arrange: Unsuccessful Change (Password is incorrect)
        nameField.Change("TestUser2");
        passField.Change("@Password1");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Incorrect Password",result.Reason);
        
        _mockUserManager.ClearMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditPassword component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditPassword()
    {
        UserSettingsTests_TabToggle<CompEditPassword>();
        await UserSettingsTests_NoDataChangeFunction<CompEditPassword>();
        
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);

        UserUpdateResult? result = null;
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        var component = parentComponent.FindComponent<CompEditPassword>();
        
        var currpass = component.Find("#old-password-field");
        var newpass = component.Find("#new-password-field");
        var confirm = component.Find("#confirm-password-field");
        var submit = component.Find("#submit-button");
        
        // Arrange: Correct Input
        currpass.Change("@Password0");
        newpass.Change("@Password1");
        confirm.Change("@Password1");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Password,result.DataType);
        Assert.True(result.Success);
        Assert.Equal("",result.Reason);
        
        // Arrange: Incorrect Input (Wrong Password)
        currpass.Change("@Password1");
        newpass.Change("@Password1");
        confirm.Change("@Password1");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Password,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Error: Password does not match.",result.Reason);
        
        _mockUserManager.ClearMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditEmail component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditEmail()
    {
        UserSettingsTests_TabToggle<CompEditEmail>();
        await UserSettingsTests_NoDataChangeFunction<CompEditEmail>();
        
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);

        UserUpdateResult? result = null;
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        var component = parentComponent.FindComponent<CompEditEmail>();
        
        var field = component.Find("#email-field");
        var submit = component.Find("#submit-button");
        
        // Arrange: Different, valid email
        field.Change("testuserzero@hotmail.com");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Email,result.DataType);
        Assert.True(result.Success);
        Assert.Equal("",result.Reason);
        
        // Arrange: Same email.
        field.Change($"test.user0@gmail.com");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Email,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Email Invalid Or Same",result.Reason);
        
        _mockUserManager.ClearMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditDisplayName component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditDisplayName()
    {
        UserSettingsTests_TabToggle<CompEditDisplayName>();
        await UserSettingsTests_NoDataChangeFunction<CompEditDisplayName>();
        
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);

        var parentComponent = Ctx.RenderComponent<CompSettings>();
        var component = parentComponent.FindComponent<CompEditDisplayName>();
        
        var field = component.Find("#displayname-field");
        var submit = component.Find("#submit-button");
        
        // TODO Impl Tests  
        Assert.Fail();
        _mockUserManager.ClearMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditBio component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditBio()
    {
        UserSettingsTests_TabToggle<CompEditBio>();
        await UserSettingsTests_NoDataChangeFunction<CompEditBio>();
        
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);
        
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        var component = parentComponent.FindComponent<CompEditBio>();
        
        var field = component.Find("#bio-field");
        var submit = component.Find("#submit-button");
        
        // TODO Impl Tests  
        Assert.Fail();
        _mockUserManager.ClearMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditAvatar component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditAvatar()
    {
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);
        
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        var component = parentComponent.FindComponent<CompEditAvatar>();
        
        var field = component.Find("#avatar-field");
        var display = component.Find("#avatar-display");
        
        // TODO Impl Tests   
        Assert.Fail();
        _mockUserManager.ClearMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompDeleteAccount component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompDeleteAccount()
    {
        UserSettingsTests_TabToggle<CompDeleteAccount>();
        await UserSettingsTests_NoDataChangeFunction<CompDeleteAccount>();
        
        var user = _mockUserManager.GetMockUser(0);
        _mockAuthenticationProvider.SetUser(user);

        UserUpdateResult? result = null;
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        var component = parentComponent.FindComponent<CompDeleteAccount>();
        
        var field = component.Find("#password-field");
        var submit = component.Find("#submit-button");
        
        // Arrange: Correct Input
        field.Change("@Password0");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Deletion,result.DataType);
        Assert.True(result.Success);
        Assert.Equal("",result.Reason);
        
        // Arrange: Incorrect Input
        field.Change("@Password1");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Deletion,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Incorrect Password",result.Reason);
        
        
        _mockUserManager.ClearMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the TabToggle component class and subclasses.
    /// </summary>
    private void UserSettingsTests_TabToggle<T>() where T : TabToggle
    {
        var component = Ctx.RenderComponent<T>();
        
        var viewTab = component.Find("#view-panel");
        var editTab = component.Find("#edit-panel"); 
        
        Assert.NotNull(viewTab);
        Assert.NotNull(editTab);

        /* Blazorise has "show" as part of tab class when visible" */
        const string visClass = "show";
        CheckElementClassContains(viewTab,visClass);
        CheckElementClassContains(editTab,visClass,true);

        component.Find("#toggle-edit").Click();
        CheckElementClassContains(viewTab,visClass,true);
        CheckElementClassContains(editTab,visClass);
        
        component.Find("#toggle-view").Click();
        CheckElementClassContains(viewTab,visClass);
        CheckElementClassContains(editTab,visClass,true);
    }

    /// <summary>
    /// Tests the functionality of the EditField deriving classes when there is no DataChangeFunction callback.
    /// </summary>
    private async Task UserSettingsTests_NoDataChangeFunction<T>() where T : EditField
    {
        StatusArguments? args = null;

        var component = Ctx.RenderComponent<T>();
        component.SetParametersAndRender(parameters 
                => parameters.Add(p => p.StatusChangeEvent, arguments => args = arguments)
        );
        
        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync( async () =>
        {
            await component.Instance.OnValidSubmitAsync();
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        Assert.NotNull(args);
        Assert.Equal(args,StatusArguments.BasicStatusArgs);
    }
    
    /// <summary>
    /// Checks whether an IElement has a given class designation.
    /// </summary>
    private static void CheckElementClassContains(IElement e, string whichClass, bool notContains = false)
    {
        var clazz = e.GetAttribute(default, "class") ?? "";
        if (notContains)
            Assert.DoesNotContain(whichClass,clazz);
        else
            Assert.Contains(whichClass, clazz);
    }
}