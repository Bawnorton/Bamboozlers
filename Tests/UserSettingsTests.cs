using AngleSharp.Dom;
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services;
using Bamboozlers.Components.Settings;
using Bamboozlers.Components.Settings.EditFields;
using Blazorise;
using Microsoft.AspNetCore.Identity;
using Blazorise.Tests.bUnit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;
using CompEditAvatar = Bamboozlers.Components.Settings.EditFields.CompEditAvatar;

namespace Tests;

public class UserSettingsTests : BlazoriseTestBase
{
    private readonly MockDatabaseProvider _mockDatabaseProvider;
    private readonly MockAuthenticationProvider _mockAuthenticationProvider;
    private readonly MockUserManager _mockUserManager;
    
    public UserSettingsTests()
    {
        _mockUserManager = new MockUserManager(Ctx);
        _mockDatabaseProvider = new MockDatabaseProvider(Ctx);
        //_mockAuthenticationProvider = new MockAuthenticationProvider(Ctx, _user.UserName!);
        
        //AuthHelper.Init(_mockAuthenticationProvider.GetAuthStateProvider(), _mockDatabaseProvider.GetDbContextFactory());
    }
    
    [Fact]
    public async Task UserSettingsTests_CompSettings()
    {
        Assert.Fail();
        // TODO Impl Tests  
    }
    
    [Fact]
    public async Task UserSettingsTests_CompEditUsername()
    {
        _mockUserManager.ClearMockUsers();
        
        var user0 = _mockUserManager.GetMockUser(0);
        var user1 = _mockUserManager.GetMockUser(1);
        
        UserSettingsTests_TabToggle<CompEditUsername>();

        UserDataRecord? passedValue = null;
        var success = false;
        var component = Ctx.RenderComponent<CompDeleteAccount>(
            parameters
                => parameters.Add(p => p.DataChangeFunction, async record =>
                {
                    passedValue = record;
                    var result = await _mockUserManager.GetUserManager().CheckPasswordAsync(user0, record.CurrentPassword!);
                    var identityResult = await _mockUserManager.GetUserManager().SetUserNameAsync(user0, record.UserName);
                    success = result && identityResult.Succeeded;
                    return success;
                })
        );
        
        // Successful Change (Username is not taken)
        var nameField = component.Find("#username");
        var passField = component.Find("#password");
        var submit = component.Find("#submit-button");
        
        nameField.SetAttribute("value","TestUser2");
        passField.SetAttribute("value","@Password0");
        await submit.ClickAsync(new MouseEventArgs());
        
        Assert.NotNull(passedValue);
        Assert.True(success);
        
        // Unsuccessful Change (Username is taken)
    }
    
    [Fact]
    public async Task UserSettingsTests_CompEditPassword()
    {
        UserSettingsTests_TabToggle<CompEditPassword>();
        // TODO Impl Tests    
        Assert.Fail();
    }
    
    [Fact]
    public async Task UserSettingsTests_CompEditEmail()
    {
        UserSettingsTests_TabToggle<CompEditEmail>();
        // TODO Impl Tests  
        Assert.Fail();
    }
    
    [Fact]
    public async Task UserSettingsTests_CompEditDisplayName()
    {
        UserSettingsTests_TabToggle<CompEditDisplayName>();
        // TODO Impl Tests
        Assert.Fail();
    }
    
    [Fact]
    public async Task UserSettingsTests_CompEditBio()
    {
        UserSettingsTests_TabToggle<CompEditBio>();
        // TODO Impl Tests  
        Assert.Fail();
    }
    
    [Fact]
    public async Task UserSettingsTests_CompEditAvatar()
    {
        // TODO Impl Tests   
        Assert.Fail();
    }
    
    [Fact]
    public async Task UserSettingsTests_CompDeleteAccount()
    {
        UserSettingsTests_TabToggle<CompDeleteAccount>();
        
        var user = _mockUserManager.GetMockUser(0);

        UserDataRecord? passedValue = null;
        var success = false;
        var component = Ctx.RenderComponent<CompDeleteAccount>(
            parameters
                => parameters.Add(p => p.DataChangeFunction, async record =>
                {
                    passedValue = record;
                    success = await _mockUserManager.GetUserManager().CheckPasswordAsync(user, record.CurrentPassword!);
                    return success;
                })
        );

        var field = component.Find("#password");
        var submit = component.Find("#submit-button");
        
        // Correct Password Result
        Assert.Equal("@Password0",user.PasswordHash);
        field.SetAttribute("value","@Password0");
        await submit.ClickAsync(new MouseEventArgs());
        Assert.NotNull(passedValue);
        Assert.True(success);
        
        // Incorrect Password Result
        field.SetAttribute("value","@Password1");
        await submit.ClickAsync(new MouseEventArgs());
        Assert.NotNull(passedValue);
        Assert.False(success);
    }
    
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

    private static void CheckElementClassContains(IElement e, string whichClass, bool notContains = false)
    {
        var clazz = e.GetAttribute(default, "class") ?? "";
        if (notContains)
            Assert.DoesNotContain(whichClass,clazz);
        else
            Assert.Contains(whichClass, clazz);
    }
}