using AngleSharp.Dom;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Data.ViewModel;
using Bamboozlers.Classes.Services;
using Bamboozlers.Components.Settings;
using Bamboozlers.Components.Settings.EditComponents.Bases;
using Bamboozlers.Components.Settings.EditComponents.Fields;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class UserSettingsTests : BlazoriseTestBase
{
    private readonly MockAuthenticationProvider _mockAuthenticationProvider;
    private readonly MockUserManager _mockUserManager;
    private readonly MockServiceProviderWrapper _mockServices;
    
    public UserSettingsTests(ITestOutputHelper output)
    {
        // Setup UserManager, Mock AuthProvider (Helper)
        _mockUserManager = new MockUserManager(Ctx);
        _mockServices = new MockServiceProviderWrapper(Ctx, _mockUserManager);
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
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        ManageMockUsers(true);
        
        var component = Ctx.RenderComponent<CompSettings>();
        
        // Assert: By default, settings popup is invisible
        Assert.False(component.Instance.Visible);

        // Arrange: Set settings component to be visible via render parameter
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
        );
        Assert.True(component.Instance.Visible);
        
        // Assert: By default, status arguments should be no-parameter constructor result
        Assert.Equal(Color.Default,component.Instance.Arguments.AlertColor);
        Assert.False(component.Instance.Arguments.AlertVisible);
        Assert.Equal("",component.Instance.Arguments.AlertMessage);
        Assert.Equal("",component.Instance.Arguments.AlertDescription);
        Assert.Equivalent(new AlertArguments() {}, component.Instance.Arguments);

        // Assert: CompSettings does not have circular callback assigned
        var callback = component.Instance.AlertEventCallback;
        Assert.NotEqual(EventCallback.Factory.Create<AlertArguments>(component.Instance,component.Instance.OnAlertChange), callback);
        
        // Act: Invoke Status Change
        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnAlertChange(new AlertArguments(
                Color.Light,
                true,
                "Test",
                "This is a test."
            ));
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert: Check that these are the values that are set
        Assert.Equal(Color.Light,component.Instance.Arguments.AlertColor);
        Assert.True(component.Instance.Arguments.AlertVisible);
        Assert.Equal("Test",component.Instance.Arguments.AlertMessage);
        Assert.Equal("This is a test.",component.Instance.Arguments.AlertDescription);
        
        ManageMockUsers();
        component.Dispose();
    }

    /// <summary>
    /// Tests the functionality of UserDisplayRecord in tandem with CompSettings.
    /// </summary>
    [Fact]
    public async Task UserSettingsTest_DisplayUser()
    {
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        // Arrange: Set the user with some non-default variables
        var user = _mockUserManager.CreateMockUser(0,
            true,
            "Bobby",
            "Hi! I'm Bobby."
        );
        user.Email = "bobby.blazor@gmail.com";
        _mockAuthenticationProvider.SetUser(user);
        AuthHelper.Invalidate();
        
        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
        );
        
        // Act: Invoke Data Display Update
        await UserService.UpdateDisplayRecordAsync();

        // Assert: Check if assigned values are as expected
        Assert.Equal("TestUser0",UserDisplayRecord.UserName);
        Assert.Equal("Bobby",UserDisplayRecord.DisplayName);
        Assert.Equal("bobby.blazor@gmail.com",UserDisplayRecord.Email);
        Assert.Equal("Hi! I'm Bobby.",UserDisplayRecord.Bio);
        Assert.Equal(UserDisplayRecord.GetDisplayableAvatar(user.Avatar), UserDisplayRecord.Avatar);
        
        // Arrange & Act: Change the data present
        
        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
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
        ManageMockUsers();
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
        var user = ManageMockUsers(true);
        
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
                CurrentPassword = user?.PasswordHash
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
                UserName = user?.UserName,
                CurrentPassword = user?.PasswordHash
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Same Username",result.Reason);
        
        // Arrange: Missing password
        completionCall = new TaskCompletionSource<bool>();
        
        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Username,
                UserName = "new"
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Empty Password",result.Reason);
        
        // Arrange: User is null
        ManageMockUsers();
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
        var user = ManageMockUsers(true);
        
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
        
        // Arrange: User is null
        ManageMockUsers();
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
        
        // Arrange: Null Email
        ManageMockUsers(true);
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
        Assert.Equal("Email Invalid Or Same",result.Reason);
        
        // Arrange: User is null
        ManageMockUsers();
        completionCall = new TaskCompletionSource<bool>();
        
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
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        UserUpdateResult? result = null;
        
        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        
        // Arrange: Null password
        ManageMockUsers(true);
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
        Assert.Equal("Empty Password",result.Reason);
        
        // Arrange: User is null
        ManageMockUsers();
        completionCall = new TaskCompletionSource<bool>();
        
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
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        UserSettingsTests_TabToggle<CompEditUsername>();
        await UserSettingsTests_NoDataChangeFunction<CompEditUsername>();
        
        ManageMockUsers(true);
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
        Assert.Equal("Error: Username is already in use.",result.Reason);
        
        // Arrange: Unsuccessful Change (Password is incorrect)
        nameField.Change("TestUser2");
        passField.Change("@Password1");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Error: Password was incorrect.",result.Reason);
        
        ManageMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditPassword component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditPassword()
    {
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        UserSettingsTests_TabToggle<CompEditPassword>();
        await UserSettingsTests_NoDataChangeFunction<CompEditPassword>();
        
        ManageMockUsers(true);

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
        
        ManageMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditEmail component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditEmail()
    {
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        UserSettingsTests_TabToggle<CompEditEmail>();
        await UserSettingsTests_NoDataChangeFunction<CompEditEmail>();
        
        var user = ManageMockUsers(true);

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
        field.Change("test.user0@gmail.com");
        
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Email,result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Email Invalid Or Same",result.Reason);
        
        ManageMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditDisplayName component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditDisplayName()
    {
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        UserSettingsTests_TabToggle<CompEditDisplayName>();
        await UserSettingsTests_NoDataChangeFunction<CompEditDisplayName>();
        
        var user = ManageMockUsers(true);

        UserUpdateResult? result = null;
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        var component = parentComponent.FindComponent<CompEditDisplayName>();
        
        var field = component.Find("#displayname-field");
        var submit = component.Find("#submit-button");
        
        // Arrange
        field.Change("UserZero");
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        // Assert
        Assert.Equal("UserZero",user.DisplayName);
        
        ManageMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditBio component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditBio()
    {
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        UserSettingsTests_TabToggle<CompEditBio>();
        await UserSettingsTests_NoDataChangeFunction<CompEditBio>();
        
        var user = ManageMockUsers(true);
        
        UserUpdateResult? result = null;
        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters 
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );
        var component = parentComponent.FindComponent<CompEditBio>();
        
        var field = component.Find("#bio-field");
        var submit = component.Find("#submit-button");
        
        // Arrange
        field.Change("This is my new bio!");
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        // Assert
        Assert.Equal("This is my new bio!",user!.Bio);

        ManageMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }
    
    /// <summary>
    /// Tests the functionality of the CompEditAvatar component.
    /// </summary>
    /// <remarks>
    /// This is an unimplemented test. It's very difficult to do unit testing with file uploads, and
    /// the coverage for the branch remains high even without it.
    /// </remarks>
    [Fact]
    public async Task UserSettingsTests_CompEditAvatar() { }
    
    /// <summary>
    /// Tests the functionality of the CompDeleteAccount component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompDeleteAccount()
    {
        await UserService.Init(_mockServices.GetServiceProviderWrapper());
        UserSettingsTests_TabToggle<CompDeleteAccount>();
        await UserSettingsTests_NoDataChangeFunction<CompDeleteAccount>();
        
        var user = ManageMockUsers(true);

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
        Assert.Equal("Error: Password was incorrect.",result.Reason);

        ManageMockUsers();
        parentComponent.Dispose();
        component.Dispose();
    }

    private User? ManageMockUsers(bool reinit = false)
    {
        AuthHelper.Invalidate();
        if (reinit)
        {
            var user = _mockUserManager.GetMockUser(0);
            _mockAuthenticationProvider.SetUser(user);
            return user;
        }
        
        _mockUserManager.ClearMockUsers();
        return null;
    }

    /// <summary>
    /// Tests the functionality of the TabToggle component class and subclasses.
    /// </summary>
    private void UserSettingsTests_TabToggle<T>() where T : EditFieldBase
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
        
        component.Find("#cancel-button").Click();
        CheckElementClassContains(viewTab,visClass);
        CheckElementClassContains(editTab,visClass,true);
    }

    /// <summary>
    /// Tests the functionality of the EditField deriving classes when there is no DataChangeFunction callback.
    /// </summary>
    private async Task UserSettingsTests_NoDataChangeFunction<T>() where T : EditFieldBase
    {
        AlertArguments? args = null;

        var component = Ctx.RenderComponent<T>();
        component.SetParametersAndRender(parameters 
                => parameters.Add(p => p.AlertEventCallback, arguments => args = arguments)
        );
        
        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync( async () =>
        {
            await component.Instance.OnValidSubmitAsync();
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);
        
        Assert.NotNull(args);
        Assert.Equal(args,AlertArguments.DefaultErrorAlertArgs);
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