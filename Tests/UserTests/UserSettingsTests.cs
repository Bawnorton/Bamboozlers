using AngleSharp.Dom;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Components.Settings;
using Bamboozlers.Components.Settings.EditComponents.Bases;
using Bamboozlers.Components.Settings.EditComponents.Fields;
using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;

namespace Tests.UserTests;

public class UserSettingsTests : AuthenticatedBlazoriseTestBase
{
    public UserSettingsTests()
    {
        // Set-up Mock Services
        MockDatabaseProvider = new MockDatabaseProvider(Ctx);
        MockAuthenticationProvider = new MockAuthenticationProvider(Ctx);
        MockUserManager = new MockUserManager(Ctx, MockDatabaseProvider);
        var mockHttpContextAccessor = new Mock<HttpContextAccessor>();
        
        // Set-up true Auth and User Services
        AuthService = new AuthService(MockAuthenticationProvider.GetAuthStateProvider(), mockHttpContextAccessor.Object, MockDatabaseProvider.GetDbContextFactory());
        UserService = new UserService(AuthService, new MockServiceProviderWrapper(Ctx, MockUserManager).GetServiceProviderWrapper(), MockDatabaseProvider.GetDbContextFactory(), UserGroupService);

        Ctx.Services.AddSingleton<IUserService>(UserService);
        Ctx.Services.AddSingleton<IAuthService>(AuthService);

        // Add Blazorise Necessities
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());

        MockUserManager.ClearMockUsers();
    }

    /// <summary>
    ///     Tests the functionality of CompSettings at large.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompSettings()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        var component = Ctx.RenderComponent<CompSettings>();

        // Assert: By default, settings popup is invisible
        Assert.False(component.Instance.Visible);

        // Arrange: Set settings component to be visible via render parameter
        component.SetParametersAndRender(parameters
            => parameters.Add(p => p.Visible, true)
        );
        Assert.True(component.Instance.Visible);

        // Assert: By default, status arguments should be no-parameter constructor result
        Assert.Equal(Color.Default, component.Instance.Arguments.AlertColor);
        Assert.False(component.Instance.Arguments.AlertVisible);
        Assert.Equal("", component.Instance.Arguments.AlertMessage);
        Assert.Equal("", component.Instance.Arguments.AlertDescription);
        Assert.Equivalent(new AlertArguments(), component.Instance.Arguments);

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
        Assert.Equal(Color.Light, component.Instance.Arguments.AlertColor);
        Assert.True(component.Instance.Arguments.AlertVisible);
        Assert.Equal("Test", component.Instance.Arguments.AlertMessage);
        Assert.Equal("This is a test.", component.Instance.Arguments.AlertDescription);

        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of UserRecord in tandem with CompSettings.
    /// </summary>
    [Fact]
    public async Task UserSettingsTest_DisplayUser()
    {
        // Arrange: Set the user with some non-default variables
        var user = MockUserManager.CreateMockUser(0,
            true,
            "bobby.blazor@gmail.com",
            "Bobby",
            "Hi! I'm Bobby."
        );

        await SetUser(user);

        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters
            => parameters.Add(p => p.Visible, true)
        );

        // Act: Invoke Data Display Update
        var data = await UserService.GetUserDataAsync();

        // Assert: Check if assigned values are as expected
        Assert.Equal("TestUser0", data.UserName);
        Assert.Equal("Bobby", data.DisplayName);
        Assert.Equal("bobby.blazor@gmail.com", data.Email);
        Assert.Equal("Hi! I'm Bobby.", data.Bio);
        Assert.Equal(user.Avatar, data.AvatarBytes);

        // Arrange & Act: Change the data present
        var success = false;
        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            success = await component.Instance.OnDataChange(new UserDataRecord
            {
                DisplayName = "Robert",
                Bio = "Hello, my name is Robert.",
                DataType = UserDataType.Visual
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);

        // Assert
        Assert.True(success);
        data = await UserService.GetUserDataAsync();

        Assert.Equal("TestUser0", data.UserName);
        Assert.Equal("Robert", data.DisplayName);
        Assert.Equal("bobby.blazor@gmail.com", data.Email);
        Assert.Equal("Hello, my name is Robert.", data.Bio);

        // Arrange & Act: Destroy Bobby "Robert" Blazor for the purposes of testing
        await SetUser(null);
        completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord { DataType = UserDataType.Visual });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);

        // Assert: That since his information is nullified, nothing should change.
        data = await UserService.GetUserDataAsync();

        Assert.Equal("N/A", data.UserName);
        Assert.Equal("N/A", data.DisplayName);
        Assert.Equal("N/A", data.Email);
        Assert.Equal("N/A", data.Bio);
    }

    /// <summary>
    ///     Tests the specific functionality of the ChangeUsername() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_ChangeUsername()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

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
                CurrentPassword = Self?.PasswordHash
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Invalid Or Empty", result.Reason);

        // Arrange: Username was set to be changed, but the new username is the same as the old.
        completionCall = new TaskCompletionSource<bool>();

        // Act
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnDataChange(new UserDataRecord
            {
                DataType = UserDataType.Username,
                UserName = Self?.UserName,
                CurrentPassword = Self?.PasswordHash
            });
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Same Username", result.Reason);

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
        Assert.Equal(UserDataType.Username, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Empty Password", result.Reason);

        // Arrange: User is null
        await SetUser(null);
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
        Assert.Equal(UserDataType.Username, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found", result.Reason);

        component.Dispose();
    }

    /// <summary>
    ///     Tests the specific functionality of the ChangePassword() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_ChangePassword()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

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
        Assert.Equal(UserDataType.Password, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Missing Input", result.Reason);

        // Arrange: User is null
        await SetUser(null);
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
        Assert.Equal(UserDataType.Password, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found", result.Reason);

        component.Dispose();
    }

    /// <summary>
    ///     Tests the specific functionality of the ChangeEmail() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_ChangeEmail()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        UserUpdateResult? result = null;

        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );

        // Arrange: Null Email
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
        Assert.Equal(UserDataType.Email, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Email Invalid Or Same", result.Reason);

        // Arrange: User is null
        await SetUser(null);
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
        Assert.Equal(UserDataType.Email, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found", result.Reason);

        component.Dispose();
    }

    /// <summary>
    ///     Tests the specific functionality of the ChangeUsername() method in the CompSettings component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_DeleteAccount()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        UserUpdateResult? result = null;

        var component = Ctx.RenderComponent<CompSettings>();
        component.SetParametersAndRender(parameters
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => result = record)
        );

        // Arrange: Null password
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
        Assert.Equal(UserDataType.Deletion, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Empty Password", result.Reason);

        // Arrange: User is null
        await SetUser(null);
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
        Assert.Equal(UserDataType.Deletion, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("User not found", result.Reason);

        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of the CompEditUsername component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditUsername()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        MockUserManager.CreateMockUser(1);
        await SetUser(user);

        UserSettingsTests_TabToggle<CompEditUsername>();
        await UserSettingsTests_NoDataChangeFunction<CompEditUsername>();

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
        Assert.Equal(UserDataType.Username, result.DataType);
        Assert.True(result.Success);
        Assert.Equal("", result.Reason);

        // Arrange: Unsuccessful Change (Username is taken)
        nameField.Change("TestUser1");
        passField.Change("@Password0");

        // Act
        await submit.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Error: Username is already in use.", result.Reason);

        // Arrange: Unsuccessful Change (Password is incorrect)
        nameField.Change("TestUser2");
        passField.Change("@Password1");

        // Act
        await submit.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Username, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Error: Password was incorrect.", result.Reason);

        parentComponent.Dispose();
        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of the CompEditPassword component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditPassword()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        UserSettingsTests_TabToggle<CompEditPassword>();
        await UserSettingsTests_NoDataChangeFunction<CompEditPassword>();

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
        Assert.Equal(UserDataType.Password, result.DataType);
        Assert.True(result.Success);
        Assert.Equal("", result.Reason);

        // Arrange: Incorrect Input (Wrong Password)
        currpass.Change("@Password1");
        newpass.Change("@Password1");
        confirm.Change("@Password1");

        // Act
        await submit.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Password, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Error: Password does not match.", result.Reason);

        parentComponent.Dispose();
        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of the CompEditEmail component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditEmail()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        UserSettingsTests_TabToggle<CompEditEmail>();
        await UserSettingsTests_NoDataChangeFunction<CompEditEmail>();

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
        Assert.Equal(UserDataType.Email, result.DataType);
        Assert.True(result.Success);
        Assert.Equal("", result.Reason);

        // Arrange: Same email.
        field.Change("test.user0@gmail.com");

        // Act
        await submit.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Email, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Email Invalid Or Same", result.Reason);

        parentComponent.Dispose();
        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of the CompEditDisplayName component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditDisplayName()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        UserSettingsTests_TabToggle<CompEditDisplayName>();
        await UserSettingsTests_NoDataChangeFunction<CompEditDisplayName>();

        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => _ = record)
        );
        var component = parentComponent.FindComponent<CompEditDisplayName>();

        var field = component.Find("#displayname-field");
        var submit = component.Find("#submit-button");

        // Arrange
        field.Change("UserZero");
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        // Assert
        Assert.Equal("UserZero", Self!.DisplayName);

        parentComponent.Dispose();
        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of the CompEditBio component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditBio()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        UserSettingsTests_TabToggle<CompEditBio>();
        await UserSettingsTests_NoDataChangeFunction<CompEditBio>();

        var parentComponent = Ctx.RenderComponent<CompSettings>();
        parentComponent.SetParametersAndRender(parameters
            => parameters.Add(p => p.Visible, true)
                .Add(p => p.UserUpdateCallback, record => _ = record)
        );
        var component = parentComponent.FindComponent<CompEditBio>();

        var field = component.Find("#bio-field");
        var submit = component.Find("#submit-button");

        // Arrange
        field.Change("This is my new bio!");
        // Act
        await submit.ClickAsync(new MouseEventArgs());
        // Assert
        var data = await UserService.GetUserDataAsync();
        Assert.Equal("This is my new bio!", Self!.Bio);
        Assert.Equal("This is my new bio!", data.Bio);

        parentComponent.Dispose();
        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of the CompEditAvatar component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompEditAvatar()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        user.Avatar = Array.Empty<byte>();
        MockDatabaseProvider.GetMockAppDbContext().MockUsers.UpdateMock(user);
        await SetUser(user);

        AlertArguments? resultArgs = null;
        UserDataRecord? dataRecord = null;
        // Arrange: Data Change Function is present
        var component = Ctx.RenderComponent<CompEditAvatar>();
        component.SetParametersAndRender(parameters
            =>
        {
            parameters.Add(p => p.AlertEventCallback, arguments => resultArgs = arguments);
            parameters.Add(p => p.DataChangeFunction, Value);
            return;

            Task<bool> Value(UserDataRecord arg)
            {
                dataRecord = arg;
                return Task.FromResult(true);
            }
        });
        
        // Act & Assert
        var toPass = new byte[] { 1 };
        var res = await component.Instance.OnChange(toPass);
        Assert.True(res);
        Assert.NotNull(dataRecord);
        Assert.Equal(UserDataType.Visual, dataRecord.DataType);
        Assert.Equal(toPass, dataRecord.AvatarBytes);
        dataRecord = null;
        
        // Act & Assert
        await component.Instance.DeleteAvatar();
        Assert.NotNull(dataRecord);
        Assert.NotNull(resultArgs);
        Assert.Equal(UserDataType.Visual, dataRecord.DataType);
        Assert.Equal(Array.Empty<byte>(), dataRecord.AvatarBytes);
        Assert.Equal("Success!", resultArgs.AlertMessage);
        Assert.Equal("Your avatar has been removed.", resultArgs.AlertDescription);
        
        // Arrange : Data Change Function is not present
        dataRecord = null;
        component.Dispose();
        component = Ctx.RenderComponent<CompEditAvatar>(parameters 
            =>
        {
            parameters.Add(p => p.AlertEventCallback, arguments => resultArgs = arguments);
        });

        res = await component.Instance.OnChange(Array.Empty<byte>());
        Assert.False(res);
        
        await component.Instance.DeleteAvatar();
        Assert.NotNull(resultArgs);
        Assert.Equal(AlertArguments.DefaultErrorAlertArgs.AlertDescription, resultArgs.AlertDescription);
        
        // Arrange: User's Avatar is null (can't delete something if it's non-existent)
        user.Avatar = null;
        dataRecord = null;
        // Act
        MockDatabaseProvider.GetMockAppDbContext().MockUsers.UpdateMock(user);
        await SetUser(user);
        await UserService.RebuildAndNotify(true);
        await component.Instance.DeleteAvatar();
        // Assert
        Assert.Null(dataRecord);
        Assert.Equal(AlertArguments.DefaultErrorAlertArgs.AlertDescription, resultArgs.AlertDescription);
    }

    /// <summary>
    ///     Tests the functionality of the CompDeleteAccount component.
    /// </summary>
    [Fact]
    public async Task UserSettingsTests_CompDeleteAccount()
    {
        MockUserManager.ClearMockUsers();
        var user = MockUserManager.CreateMockUser(0);
        await SetUser(user);

        UserSettingsTests_TabToggle<CompDeleteAccount>();
        await UserSettingsTests_NoDataChangeFunction<CompDeleteAccount>();

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
        Assert.Equal(UserDataType.Deletion, result.DataType);
        Assert.True(result.Success);
        Assert.Equal("", result.Reason);

        // Arrange: Incorrect Input
        field.Change("@Password1");

        // Act
        await submit.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserDataType.Deletion, result.DataType);
        Assert.False(result.Success);
        Assert.Equal("Error: Password was incorrect.", result.Reason);

        parentComponent.Dispose();
        component.Dispose();
    }

    /// <summary>
    ///     Tests the functionality of the TabToggle component class and subclasses.
    /// </summary>
    private void UserSettingsTests_TabToggle<T>() where T : EditFieldBase
    {
        var component = Ctx.RenderComponent<T>();

        var viewTab = component.Find("#view-panel");
        var editTab = component.Find("#edit-panel");

        Assert.NotNull(viewTab);
        Assert.NotNull(editTab);

        /* Blazorise has "show" as part of tab class when visible */
        const string visClass = "show";
        CheckElementClassContains(viewTab, visClass);
        CheckElementClassContains(editTab, visClass, true);

        component.Find("#toggle-edit").Click();
        CheckElementClassContains(viewTab, visClass, true);
        CheckElementClassContains(editTab, visClass);

        component.Find("#cancel-button").Click();
        CheckElementClassContains(viewTab, visClass);
        CheckElementClassContains(editTab, visClass, true);
    }

    /// <summary>
    ///     Tests the functionality of the EditField deriving classes when there is no DataChangeFunction callback.
    /// </summary>
    private async Task UserSettingsTests_NoDataChangeFunction<T>() where T : EditFieldBase
    {
        AlertArguments? args = null;

        var component = Ctx.RenderComponent<T>();
        component.SetParametersAndRender(parameters
            => parameters.Add(p => p.AlertEventCallback, arguments => args = arguments)
        );

        var completionCall = new TaskCompletionSource<bool>();
        await component.InvokeAsync(async () =>
        {
            await component.Instance.OnValidSubmitAsync();
            completionCall.SetResult(true);
        });
        await completionCall.Task.WaitAsync(CancellationToken.None);

        Assert.NotNull(args);
        Assert.Equal(args, AlertArguments.DefaultErrorAlertArgs);
    }

    /// <summary>
    ///     Checks whether an IElement has a given class designation.
    /// </summary>
    private static void CheckElementClassContains(IElement e, string whichClass, bool notContains = false)
    {
        var clazz = e.GetAttribute(default, "class") ?? "";
        if (notContains)
            Assert.DoesNotContain(whichClass, clazz);
        else
            Assert.Contains(whichClass, clazz);
    }
}

public class MockBrowserFile : IBrowserFile
{
    public byte[] Bytes { get; init; } = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    public string Name => "MockImage";
    public DateTimeOffset LastModified { get; } = DateTimeOffset.Now;
    public long Size => Bytes.Length;
    public string ContentType { get; init; } = "image/png";

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = new())
    {
        return new MemoryStream(Bytes);
    }
}