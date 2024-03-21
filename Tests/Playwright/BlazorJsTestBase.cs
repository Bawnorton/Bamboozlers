using Bamboozlers;
using Bamboozlers.Account;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Service;
using Bamboozlers.Classes.Services;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using IMessageService = Bamboozlers.Classes.Services.IMessageService;

namespace Tests.Playwright;

public class BlazorJsTestBase(bool headless = true, string username = "testuser", string password = "Hpxd3910!") : PageTest
{
    protected WebApplication? Host;
    
    protected Uri RootUri { get; private set; } = default!;
    
    [SetUp]
    public async Task SetUpWebApplication()
    {
        var builder = WebApplication.CreateBuilder();
        
        var configuration = builder.Configuration;

        // Add services to the container.
        builder.Services
            .AddBlazorise( options => options.Immediate = true )
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IIdentityRedirectManager, IdentityRedirectManager>();
        builder.Services.AddScoped<IdentityRedirectManagerWrapper>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<IMessageService, MessageService>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CONNECTION_STRING")));

        builder.Services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddTransient<IEmailSender<User>, EmailSender>();
        builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
            options.InstanceName = "SampleInstance";
        });
        
        const string url = "http://localhost:5001";
        builder.WebHost.UseUrls(url);
        builder.WebHost.UseStaticWebAssets();
        Host = builder.Build();

        Host.UseExceptionHandler("/Error", true);
        Host.UseHsts();
        Host.UseHttpsRedirection();
        Host.UseStaticFiles();
        Host.UseAntiforgery();
        Host.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        Host.MapAdditionalIdentityEndpoints();
        
        await Host.StartAsync();
        
        RootUri = new Uri(url);

        // Set up Playwright
        var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });
        var page = await browser.NewPageAsync();

        await CreateUserIfNotExists();
        
        // Log in
        await page.GotoAsync(RootUri.ToString());
        await page.GetByPlaceholder("name@example.com or username").FillAsync(username);
        await page.GetByPlaceholder("password").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log in" }).ClickAsync();
    }

    private async Task CreateUserIfNotExists()
    {
        var userManager = Host!.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            user = new User
            {
                UserName = username,
                Email = $"{username}@gmail.com",
                DisplayName = username.First().ToString().ToUpper() + username[1..],
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, password);
        }
    }

    [TearDown]
    public async Task TearDownWebApplication()
    {
        if (Host is not null)
        {
            await Host.StopAsync();
            ((IDisposable) Host).Dispose();
        }
    }
}