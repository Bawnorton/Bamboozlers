using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using NUnit.Framework;
using Tests.Playwright.Infrastructure;

namespace Tests.Playwright;

[Parallelizable(ParallelScope.Self)]
public class PlaywrightTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly string ServerAddress;
    protected readonly IServiceProvider ServiceProvider;
    
    protected const string TestUserName = "testuser";
    protected const string TestUserPassword = "Password123!";
    protected const string TestUserEmail = "testuser@gmail.com";

    protected User? Self;
    
    protected PlaywrightTestBase(CustomWebApplicationFactory fixture)
    {
        ServerAddress = fixture.ServerAddress;
        ServiceProvider = fixture.Services;
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
        if (!result.Succeeded) throw new Exception("Failed to create user");
        return user;
    }
}