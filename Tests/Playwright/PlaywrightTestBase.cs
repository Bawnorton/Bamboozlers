using System.Diagnostics;
using System.Net.NetworkInformation;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Tests.Playwright.Infrastructure;
using Xunit.Abstractions;

namespace Tests.Playwright;

public class PlaywrightTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly string ServerAddress;
    protected readonly IServiceProvider ServiceProvider;
    
    protected IPlaywright? Playwright;
    protected IBrowser? Browser;
    
    protected const string TestUserName = "testuser";
    protected const string TestUserPassword = "Password123!";
    protected const string TestUserEmail = "testuser@gmail.com";

    protected User? Self;
    
    protected PlaywrightTestBase(CustomWebApplicationFactory fixture, ITestOutputHelper outputHelper)
    {
        ServerAddress = fixture.ServerAddress;
        ServiceProvider = fixture.Services;
        
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnections = ipGlobalProperties.GetActiveTcpConnections();
        if (tcpConnections.Any(connection => connection.LocalEndPoint.Port == 5180))
        {
            outputHelper.WriteLine("Found existing websocket server on port 5180, skipping server start");
            return;
        }
        
        var websocketProcess = new Process();
        websocketProcess.StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            Arguments = "-c \"cd ../../../../Backend && uvicorn main:app --reload --port 5180\""
        };
        Task.Run(() => // Start the websocket server in a separate thread
        {
            websocketProcess.Start();
            var websocketProcessId = websocketProcess.Id;
            outputHelper.WriteLine($"Started websocket server on port 5180 with PID {websocketProcessId}");
            
            fixture.Disposing += (_, _) => websocketProcess.Kill();
        });
    }

    protected async Task<IPage> Setup(bool headless = true)
    {
        Playwright ??= await Microsoft.Playwright.Playwright.CreateAsync();
        Browser ??= await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });
        var context = await Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync(ServerAddress);
        return page;
    }
    
    protected async Task Login(IPage page, string username = TestUserName, string email = TestUserEmail, string password = TestUserPassword)
    {
        Self = await GetOrCreateUser(username, email, password);
        await page.GetByPlaceholder("name@example.com or username").ClickAsync();
        await page.GetByPlaceholder("name@example.com or username").FillAsync(username);
        await page.GetByPlaceholder("password").ClickAsync();
        await page.GetByPlaceholder("password").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log in" }).ClickAsync();
    }
    
    protected async Task<User> GetOrCreateUser(string username, string email, string password)
    {
        using var scope = ServiceProvider.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user != null) return user;
        
        user = await CreateUser(username, email, password);
        return user;
    }
    
    protected async Task<IPage> SetupAndLogin(bool headless = true, string username = TestUserName, string email = TestUserEmail, string password = TestUserPassword)
    {
        var page = await Setup(headless);
        await Login(page, username, email, password);
        return page;
    }
    
    protected async Task<IPage> SetupAndLoginAtFirstDm(bool headless = true, string username = TestUserName, string email = TestUserEmail, string password = TestUserPassword)
    {
        var page = await SetupAndLogin(headless, username, email, password);
        await OpenFirstDm(page);
        return page;
    }
    
    protected async Task OpenFirstDm(IPage page)
    {
        await page
            .GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = " Direct Messages" })
            .ClickAsync();
        var dmEntries = page
            .Locator("#dms_dropdown")
            .Locator(".b-bar-item");

        if (dmEntries.CountAsync().Result == 0)
        {
            await CreateDm(Self!, await GetOrCreateUser("testuser2", "testuser2@gmail.com", TestUserPassword));
            await page.ReloadAsync();
            await OpenFirstDm(page);
        }
        
        await dmEntries.First.ClickAsync();
    }
    
    protected async Task CreateDm(User owner, User other)
    {
        using var scope = ServiceProvider.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var context = await contextFactory.CreateDbContextAsync();
        var chat = await context.Chats.FirstOrDefaultAsync(chat => chat.Users.Contains(owner) && chat.Users.Contains(other) && chat.Users.Count == 2);
        if (chat != null)
        {
            throw new InvalidOperationException($"Direct message already exists between {owner.UserName} and {other.UserName}"); 
        } 
        
        var dm = new Chat
        {
            Users = [owner, other],
            Messages = []
        };
        owner.Chats ??= [];
        other.Chats ??= [];
        owner.Chats.Add(dm);
        other.Chats.Add(dm);
        context.Chats.Add(dm);
        // avoid identity insertion, I have no idea why it's trying to insert new users
        context.ChangeTracker.Entries<User>().ToList().ForEach(e => e.State = EntityState.Unchanged);
        await context.SaveChangesAsync();
    }
    
    private async Task<User> CreateUser(string username, string email, string password)
    {
        var user = Activator.CreateInstance<User>();
        using var scope = ServiceProvider.CreateScope();
        var userStore = (scope.ServiceProvider.GetService<IUserStore<User>>()as IUserEmailStore<User>)!;
        await userStore.SetUserNameAsync(user, username, CancellationToken.None);
        await userStore.SetEmailAsync(user, email, CancellationToken.None);
        user.EmailConfirmed = true;
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded) return user;
        
        var error = new Exception($"Failed to create user {username} with email {email} and password {password}");
        error.Data.Add("Errors", result.Errors);
        throw error;
    }
}